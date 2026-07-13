using ISphereHub.Application.Interfaces;
using ISphereHub.BuildingBlocks.Dtos.OrderDtos;
using ISphereHub.BuildingBlocks.Exceptions;
using ISphereHub.Domain.Entities;
using ISphereHub.Domain.Enums;
using ISphereHub.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IOrderRepository orderRepository, IProductRepository productRepository, ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<OrderDto> CreateOnlineOrderAsync(CreateOnlineOrderDto dto, CancellationToken ct = default)
        {
            if (dto.Items is null || dto.Items.Count == 0)
                throw new ValidationAppException("Order must contain at least one item.");

            var orderItems = await BuildAndReserveOrderItemsAsync(dto.Items, ct);

            try
            {
                var order = Order.CreateOnlineOrder(
                    dto.CustomerName,
                    dto.PhoneNumber,
                    dto.Location,
                    dto.Address,
                    orderItems,
                    dto.Notes);

                await _orderRepository.CreateAsync(order, ct);
                _logger.LogInformation("Online order created: {OrderNumber} - total {Total}", order.OrderNumber, order.TotalAmount);

                return MapToDto(order);
            }
            catch
            {
                // If order persistence fails after stock was already deducted,
                // compensate by restoring the reserved stock.
                await CompensateStockAsync(orderItems, ct);
                throw;
            }
        }

        public async Task<OrderDto> CreateInStoreSaleAsync(CreateInStoreSaleDto dto, string adminId, string adminUsername, CancellationToken ct = default)
        {
            if (dto.Items is null || dto.Items.Count == 0)
                throw new ValidationAppException("Sale must contain at least one item.");

            if (!Enum.TryParse<PaymentMethod>(dto.PaymentMethod, true, out var paymentMethod) || paymentMethod == PaymentMethod.PayOnDelivery)
                throw new ValidationAppException($"Invalid in-store payment method '{dto.PaymentMethod}'.");

            var orderItems = await BuildAndReserveOrderItemsAsync(dto.Items, ct);

            try
            {
                var order = Order.CreateInStoreSale(
                    dto.CustomerName,
                    dto.PhoneNumber,
                    orderItems,
                    paymentMethod,
                    adminId,
                    adminUsername,
                    dto.Notes);

                await _orderRepository.CreateAsync(order, ct);
                _logger.LogInformation(
                    "In-store sale recorded: {OrderNumber} - total {Total} - served by {Admin}",
                    order.OrderNumber, order.TotalAmount, adminUsername);

                return MapToDto(order);
            }
            catch
            {
                await CompensateStockAsync(orderItems, ct);
                throw;
            }
        }

        private async Task<List<OrderItem>> BuildAndReserveOrderItemsAsync(List<OrderItemRequestDto> requestedItems, CancellationToken ct)
        {
            var mergedRequests = requestedItems
                .GroupBy(i => i.ProductId)
                .Select(g => new OrderItemRequestDto { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToList();

            foreach (var item in mergedRequests)
            {
                if (item.Quantity <= 0)
                    throw new ValidationAppException($"Quantity for product '{item.ProductId}' must be greater than zero.");
            }

            var productIds = mergedRequests.Select(i => i.ProductId).ToList();
            var products = await _productRepository.GetByIdsAsync(productIds, ct);
            var productMap = products.ToDictionary(p => p.Id, p => p);

            var reserved = new List<OrderItem>();

            foreach (var request in mergedRequests)
            {
                if (!productMap.TryGetValue(request.ProductId, out var product))
                {
                    await CompensateStockAsync(reserved, ct);
                    throw new NotFoundException($"Product with id '{request.ProductId}' was not found.");
                }

                if (!product.IsActive)
                {
                    await CompensateStockAsync(reserved, ct);
                    throw new ConflictException($"Product '{product.Name}' is not currently available for sale.");
                }

                var deducted = await _productRepository.AdjustStockAsync(request.ProductId, -request.Quantity, ct);
                if (!deducted)
                {
                    await CompensateStockAsync(reserved, ct);
                    throw new ConflictException(
                        $"Insufficient stock for '{product.Name}'. Only {product.StockQuantity} unit(s) available.");
                }

                reserved.Add(OrderItem.Create(
                    product.Id,
                    product.Name,
                    product.Images.FirstOrDefault(),
                    request.Quantity,
                    product.Price));
            }

            return reserved;
        }

        private async Task CompensateStockAsync(List<OrderItem> reservedItems, CancellationToken ct)
        {
            foreach (var item in reservedItems)
            {
                try
                {
                    await _productRepository.AdjustStockAsync(item.ProductId, item.Quantity, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to compensate stock rollback for product {ProductId}", item.ProductId);
                }
            }
        }

        public async Task<OrderListResponse> GetAllOrdersAsync(OrderQueryDto query, CancellationToken ct = default)
        {
            OrderStatus? status = null;
            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                if (!Enum.TryParse<OrderStatus>(query.Status, true, out var parsedStatus))
                    throw new ValidationAppException($"Invalid status '{query.Status}'.");
                status = parsedStatus;
            }

            OrderChannel? channel = null;
            if (!string.IsNullOrWhiteSpace(query.Channel))
            {
                if (!Enum.TryParse<OrderChannel>(query.Channel, true, out var parsedChannel))
                    throw new ValidationAppException($"Invalid channel '{query.Channel}'.");
                channel = parsedChannel;
            }

            var options = new OrderQueryOptions
            {
                Status = status,
                Channel = channel,
                FromDate = query.FromDate,
                ToDate = query.ToDate,
                Search = query.Search,
                Page = query.Page < 1 ? 1 : query.Page,
                PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize
            };

            var (items, totalCount) = await _orderRepository.GetAllAsync(options, ct);

            return new OrderListResponse
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = options.Page,
                PageSize = options.PageSize
            };
        }

        public async Task<OrderDto> GetOrderByIdAsync(string id, CancellationToken ct = default)
        {
            var order = await _orderRepository.GetByIdAsync(id, ct)
                ?? throw new NotFoundException($"Order with id '{id}' was not found.");

            return MapToDto(order);
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(string id, UpdateOrderStatusDto dto, CancellationToken ct = default)
        {
            if (!Enum.TryParse<OrderStatus>(dto.Status, true, out var status))
                throw new ValidationAppException($"Invalid status '{dto.Status}'.");

            var order = await _orderRepository.GetByIdAsync(id, ct)
                ?? throw new NotFoundException($"Order with id '{id}' was not found.");

            // If an order is cancelled before delivery, return the reserved stock.
            if (status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
            {
                foreach (var item in order.Items)
                {
                    await _productRepository.AdjustStockAsync(item.ProductId, item.Quantity, ct);
                }
            }

            var updated = await _orderRepository.UpdateStatusAsync(id, status, ct);
            if (!updated)
                throw new NotFoundException($"Order with id '{id}' was not found.");

            order.UpdateStatus(status);
            _logger.LogInformation("Order {OrderId} status updated to {Status}", id, status);

            return MapToDto(order);
        }

        public async Task<OrderStatsSummaryDto> GetStatsSummaryAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
        {
            var from = fromDate ?? DateTime.UtcNow.Date.AddDays(-30);
            var to = toDate ?? DateTime.UtcNow;

            var summary = await _orderRepository.GetStatsSummaryAsync(from, to, ct);

            return new OrderStatsSummaryDto
            {
                TotalOrders = summary.TotalOrders,
                TotalRevenue = summary.TotalRevenue,
                OnlineOrders = summary.OnlineOrders,
                InStoreOrders = summary.InStoreOrders,
                OnlineRevenue = summary.OnlineRevenue,
                InStoreRevenue = summary.InStoreRevenue
            };
        }

        private static OrderDto MapToDto(Order order) => new()
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerName = order.CustomerName,
            PhoneNumber = order.PhoneNumber,
            Location = order.Location,
            Address = order.Address,
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ImageUrl = i.ImageUrl,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.LineTotal
            }).ToList(),
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
            Channel = order.Channel.ToString(),
            PaymentMethod = order.PaymentMethod.ToString(),
            ServedByAdminUsername = order.ServedByAdminUsername,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt
        };
    }
}
