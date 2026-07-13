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
    public class ProductRepository : IProductRepository
    {
        private readonly IMongoCollection<Product> _products;

        public ProductRepository(MongoDbContext context)
        {
            _products = context.Products;
        }

        public async Task<(IReadOnlyList<Product> Items, long TotalCount)> GetAllAsync(ProductQueryOptions options, CancellationToken ct = default)
        {
            var filterBuilder = Builders<Product>.Filter;
            var filters = new List<FilterDefinition<Product>>();

            if (!options.IncludeInactive)
                filters.Add(filterBuilder.Eq(p => p.IsActive, true));

            if (!string.IsNullOrWhiteSpace(options.Brand) &&
                Enum.TryParse<ProductBrand>(options.Brand, true, out var brand))
                filters.Add(filterBuilder.Eq(p => p.Brand, brand));

            if (!string.IsNullOrWhiteSpace(options.Condition))
            {
                var normalizedCondition = options.Condition.Replace("-", "").Replace(" ", "");
                if (normalizedCondition.Equals("ExUK", StringComparison.OrdinalIgnoreCase))
                    normalizedCondition = "ExUk";
                if (Enum.TryParse<ProductCondition>(normalizedCondition, true, out var condition))
                    filters.Add(filterBuilder.Eq(p => p.Condition, condition));
            }

            if (!string.IsNullOrWhiteSpace(options.Category) &&
                Enum.TryParse<ProductCategory>(options.Category, true, out var category))
                filters.Add(filterBuilder.Eq(p => p.Category, category));

            if (options.MinPrice.HasValue)
                filters.Add(filterBuilder.Gte(p => p.Price, options.MinPrice.Value));

            if (options.MaxPrice.HasValue)
                filters.Add(filterBuilder.Lte(p => p.Price, options.MaxPrice.Value));

            if (options.IsFeatured.HasValue)
                filters.Add(filterBuilder.Eq(p => p.IsFeatured, options.IsFeatured.Value));

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                var searchFilter = filterBuilder.Or(
                    filterBuilder.Regex(p => p.Name, new MongoDB.Bson.BsonRegularExpression(options.Search, "i")),
                    filterBuilder.Regex(p => p.Model, new MongoDB.Bson.BsonRegularExpression(options.Search, "i"))
                );
                filters.Add(searchFilter);
            }

            var finalFilter = filters.Count > 0 ? filterBuilder.And(filters) : filterBuilder.Empty;

            var sort = options.Sort?.ToLowerInvariant() switch
            {
                "price_asc" => Builders<Product>.Sort.Ascending(p => p.Price),
                "price_desc" => Builders<Product>.Sort.Descending(p => p.Price),
                "best_selling" => Builders<Product>.Sort.Descending(p => p.UnitsSold),
                "latest" => Builders<Product>.Sort.Descending(p => p.CreatedAt),
                _ => Builders<Product>.Sort.Descending(p => p.CreatedAt)
            };

            var totalCount = await _products.CountDocumentsAsync(finalFilter, cancellationToken: ct);

            var items = await _products.Find(finalFilter)
                .Sort(sort)
                .Skip((options.Page - 1) * options.PageSize)
                .Limit(options.PageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<Product?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _)) return null;
            return await _products.Find(p => p.Id == id).FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<string> ids, CancellationToken ct = default)
        {
            var validIds = ids.Where(id => MongoDB.Bson.ObjectId.TryParse(id, out _)).ToList();
            if (validIds.Count == 0) return new List<Product>();

            return await _products.Find(Builders<Product>.Filter.In(p => p.Id, validIds)).ToListAsync(ct);
        }

        public async Task CreateAsync(Product product, CancellationToken ct = default)
        {
            await _products.InsertOneAsync(product, cancellationToken: ct);
        }

        public async Task<bool> UpdateAsync(Product product, CancellationToken ct = default)
        {
            var result = await _products.ReplaceOneAsync(p => p.Id == product.Id, product, cancellationToken: ct);
            return result.MatchedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
        {
            var result = await _products.DeleteOneAsync(p => p.Id == id, ct);
            return result.DeletedCount > 0;
        }

        public async Task<bool> AdjustStockAsync(string id, int delta, CancellationToken ct = default)
        {
            var filterBuilder = Builders<Product>.Filter;
            var filter = filterBuilder.Eq(p => p.Id, id);

            if (delta < 0)
                filter &= filterBuilder.Gte(p => p.StockQuantity, -delta);

            var update = Builders<Product>.Update
                .Inc(p => p.StockQuantity, delta)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);

            if (delta < 0)
                update = update.Inc(p => p.UnitsSold, -delta);

            var result = await _products.UpdateOneAsync(filter, update, cancellationToken: ct);
            return result.ModifiedCount > 0;
        }

        public async Task<IReadOnlyList<string>> GetDistinctBrandsAsync(CancellationToken ct = default)
        {
            var brands = await _products.DistinctAsync(p => p.Brand, Builders<Product>.Filter.Eq(p => p.IsActive, true), cancellationToken: ct);
            var result = await brands.ToListAsync(ct);
            return result.Select(b => b.ToString()).ToList();
        }
    }
}
