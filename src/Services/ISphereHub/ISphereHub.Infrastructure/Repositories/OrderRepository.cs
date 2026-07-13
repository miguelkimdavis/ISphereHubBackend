using ISphereHub.Domain.Entities;
using ISphereHub.Domain.Enums;
using ISphereHub.Domain.Interfaces;
using ISphereHub.Infrastructure.Persistence;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IMongoCollection<Order> _orders;

        public OrderRepository(MongoDbContext context)
        {
            _orders = context.Orders;
        }

        public async Task<(IReadOnlyList<Order> Items, long TotalCount)> GetAllAsync(OrderQueryOptions options, CancellationToken ct = default)
        {
            var filterBuilder = Builders<Order>.Filter;
            var filters = new List<FilterDefinition<Order>>();

            if (options.Status.HasValue)
                filters.Add(filterBuilder.Eq(o => o.Status, options.Status.Value));

            if (options.Channel.HasValue)
                filters.Add(filterBuilder.Eq(o => o.Channel, options.Channel.Value));

            if (options.FromDate.HasValue)
                filters.Add(filterBuilder.Gte(o => o.CreatedAt, options.FromDate.Value));

            if (options.ToDate.HasValue)
                filters.Add(filterBuilder.Lte(o => o.CreatedAt, options.ToDate.Value));

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                var regex = new MongoDB.Bson.BsonRegularExpression(options.Search, "i");
                filters.Add(filterBuilder.Or(
                    filterBuilder.Regex(o => o.CustomerName, regex),
                    filterBuilder.Regex(o => o.PhoneNumber, regex),
                    filterBuilder.Regex(o => o.OrderNumber, regex)
                ));
            }

            var finalFilter = filters.Count > 0 ? filterBuilder.And(filters) : filterBuilder.Empty;

            var totalCount = await _orders.CountDocumentsAsync(finalFilter, cancellationToken: ct);

            var items = await _orders.Find(finalFilter)
                .SortByDescending(o => o.CreatedAt)
                .Skip((options.Page - 1) * options.PageSize)
                .Limit(options.PageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<Order?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _)) return null;
            return await _orders.Find(o => o.Id == id).FirstOrDefaultAsync(ct);
        }

        public async Task CreateAsync(Order order, CancellationToken ct = default)
        {
            await _orders.InsertOneAsync(order, cancellationToken: ct);
        }

        public async Task<bool> UpdateStatusAsync(string id, OrderStatus status, CancellationToken ct = default)
        {
            var update = Builders<Order>.Update
                .Set(o => o.Status, status)
                .Set(o => o.UpdatedAt, DateTime.UtcNow);

            var result = await _orders.UpdateOneAsync(o => o.Id == id, update, cancellationToken: ct);
            return result.MatchedCount > 0;
        }

        public async Task<OrderStatsSummary> GetStatsSummaryAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default)
        {
            var filter = Builders<Order>.Filter.Gte(o => o.CreatedAt, fromDate) &
                         Builders<Order>.Filter.Lte(o => o.CreatedAt, toDate) &
                         Builders<Order>.Filter.Ne(o => o.Status, OrderStatus.Cancelled);

            var orders = await _orders.Find(filter).ToListAsync(ct);

            return new OrderStatsSummary
            {
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => o.TotalAmount),
                OnlineOrders = orders.Count(o => o.Channel == OrderChannel.Online),
                InStoreOrders = orders.Count(o => o.Channel == OrderChannel.InStore),
                OnlineRevenue = orders.Where(o => o.Channel == OrderChannel.Online).Sum(o => o.TotalAmount),
                InStoreRevenue = orders.Where(o => o.Channel == OrderChannel.InStore).Sum(o => o.TotalAmount)
            };
        }
    }
}
