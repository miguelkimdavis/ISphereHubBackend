using ISphereHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<(IReadOnlyList<Product> Items, long TotalCount)> GetAllAsync(ProductQueryOptions options, CancellationToken ct = default);
        Task<Product?> GetByIdAsync(string id, CancellationToken ct = default);
        Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<string> ids, CancellationToken ct = default);
        Task CreateAsync(Product product, CancellationToken ct = default);
        Task<bool> UpdateAsync(Product product, CancellationToken ct = default);
        Task<bool> DeleteAsync(string id, CancellationToken ct = default);
        Task<bool> AdjustStockAsync(string id, int delta, CancellationToken ct = default);
        Task<IReadOnlyList<string>> GetDistinctBrandsAsync(CancellationToken ct = default);
    }

    public class ProductQueryOptions
    {
        public string? Brand { get; set; }
        public string? Condition { get; set; }
        public string? Category { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Search { get; set; }
        public string? Sort { get; set; } // price_asc | price_desc | latest | best_selling
        public bool? IsFeatured { get; set; }
        public bool IncludeInactive { get; set; } = false;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
