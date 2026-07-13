using ISphereHub.BuildingBlocks.Dtos.OrderDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOnlineOrderAsync(CreateOnlineOrderDto dto, CancellationToken ct = default);
        Task<OrderDto> CreateInStoreSaleAsync(CreateInStoreSaleDto dto, string adminId, string adminUsername, CancellationToken ct = default);
        Task<OrderListResponse> GetAllOrdersAsync(OrderQueryDto query, CancellationToken ct = default);
        Task<OrderDto> GetOrderByIdAsync(string id, CancellationToken ct = default);
        Task<OrderDto> UpdateOrderStatusAsync(string id, UpdateOrderStatusDto dto, CancellationToken ct = default);
        Task<OrderStatsSummaryDto> GetStatsSummaryAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
    }
}
