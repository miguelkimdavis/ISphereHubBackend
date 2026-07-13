using ISphereHub.Domain.Entities;
using ISphereHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task<(IReadOnlyList<Order> Items, long TotalCount)> GetAllAsync(OrderQueryOptions options, CancellationToken ct = default);
        Task<Order?> GetByIdAsync(string id, CancellationToken ct = default);
        Task CreateAsync(Order order, CancellationToken ct = default);
        Task<bool> UpdateStatusAsync(string id, OrderStatus status, CancellationToken ct = default);
        Task<OrderStatsSummary> GetStatsSummaryAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default);
    }

    public class OrderStatsSummary
    {
        public long TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public long OnlineOrders { get; set; }
        public long InStoreOrders { get; set; }
        public decimal OnlineRevenue { get; set; }
        public decimal InStoreRevenue { get; set; }
    }

    public class OrderQueryOptions
    {
        public OrderStatus? Status { get; set; }
        public OrderChannel? Channel { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Search { get; set; } // matches customer name / phone / order number
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
