using ISphereHub.Application.Interfaces;
using ISphereHub.BuildingBlocks.Dtos.ProductDtos;
using ISphereHub.BuildingBlocks.Exceptions;
using ISphereHub.Domain.Entities;
using ISphereHub.Domain.Enums;
using ISphereHub.Domain.Interfaces;
using ISphereHub.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<ProductListResponse> GetAllProductsAsync(ProductQueryDto query, CancellationToken ct = default)
        {
            var options = new ProductQueryOptions
            {
                Brand = query.Brand,
                Condition = query.Condition,
                Category = query.Category,
                MinPrice = query.MinPrice,
                MaxPrice = query.MaxPrice,
                Search = query.Search,
                Sort = query.Sort,
                IsFeatured = query.IsFeatured,
                IncludeInactive = query.IncludeInactive,
                Page = query.Page < 1 ? 1 : query.Page,
                PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize
            };

            var (items, totalCount) = await _productRepository.GetAllAsync(options, ct);

            return new ProductListResponse
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = options.Page,
                PageSize = options.PageSize
            };
        }

        public async Task<ProductDto> GetProductByIdAsync(string id, CancellationToken ct = default)
        {
            var product = await _productRepository.GetByIdAsync(id, ct)
                ?? throw new NotFoundException($"Product with id '{id}' was not found.");

            return MapToDto(product);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken ct = default)
        {
            var brand = ParseEnum<ProductBrand>(dto.Brand, nameof(dto.Brand));
            var condition = ParseEnum<ProductCondition>(dto.Condition, nameof(dto.Condition));
            var category = ParseEnum<ProductCategory>(dto.Category, nameof(dto.Category));

            var product = Product.Create(
                dto.Name,
                brand,
                dto.Model,
                condition,
                category,
                dto.Price,
                dto.StockQuantity,
                dto.Images,
                MapSpecs(dto.Specs),
                dto.Description,
                dto.CompareAtPrice,
                dto.IsFeatured);

            await _productRepository.CreateAsync(product, ct);
            _logger.LogInformation("Product created: {ProductId} - {ProductName}", product.Id, product.Name);

            return MapToDto(product);
        }

        public async Task<ProductDto> UpdateProductAsync(string id, UpdateProductDto dto, CancellationToken ct = default)
        {
            var product = await _productRepository.GetByIdAsync(id, ct)
                ?? throw new NotFoundException($"Product with id '{id}' was not found.");

            var brand = ParseEnum<ProductBrand>(dto.Brand, nameof(dto.Brand));
            var condition = ParseEnum<ProductCondition>(dto.Condition, nameof(dto.Condition));
            var category = ParseEnum<ProductCategory>(dto.Category, nameof(dto.Category));

            product.UpdateDetails(
                dto.Name,
                brand,
                dto.Model,
                condition,
                category,
                dto.Price,
                dto.CompareAtPrice,
                dto.Images,
                MapSpecs(dto.Specs),
                dto.Description,
                dto.IsFeatured,
                dto.IsActive);

            var updated = await _productRepository.UpdateAsync(product, ct);
            if (!updated)
                throw new NotFoundException($"Product with id '{id}' was not found.");

            _logger.LogInformation("Product updated: {ProductId}", id);
            return MapToDto(product);
        }

        public async Task DeleteProductAsync(string id, CancellationToken ct = default)
        {
            var deleted = await _productRepository.DeleteAsync(id, ct);
            if (!deleted)
                throw new NotFoundException($"Product with id '{id}' was not found.");

            _logger.LogInformation("Product deleted: {ProductId}", id);
        }

        public async Task<ProductDto> AdjustStockAsync(string id, AdjustStockDto dto, CancellationToken ct = default)
        {
            if (dto.Delta == 0)
                throw new ValidationAppException("Stock adjustment delta must be non-zero.");

            var success = await _productRepository.AdjustStockAsync(id, dto.Delta, ct);
            if (!success)
                throw new NotFoundException($"Product with id '{id}' was not found.");

            var product = await _productRepository.GetByIdAsync(id, ct)
                ?? throw new NotFoundException($"Product with id '{id}' was not found.");

            _logger.LogInformation("Stock adjusted for {ProductId} by {Delta}. Reason: {Reason}", id, dto.Delta, dto.Reason ?? "n/a");
            return MapToDto(product);
        }

        public async Task<IReadOnlyList<string>> GetBrandsAsync(CancellationToken ct = default)
        {
            return await _productRepository.GetDistinctBrandsAsync(ct);
        }

        private static T ParseEnum<T>(string value, string fieldName) where T : struct, Enum
        {
            var normalized = value?.Trim().Replace("-", "").Replace(" ", "") ?? string.Empty;
            if (normalized.Equals("ExUK", StringComparison.OrdinalIgnoreCase))
                normalized = "ExUk";

            if (!Enum.TryParse<T>(normalized, true, out var result))
                throw new ValidationAppException($"Invalid value '{value}' for {fieldName}.");

            return result;
        }

        private static ProductSpecs MapSpecs(ProductSpecsDto dto) => new()
        {
            Ram = dto.Ram,
            Storage = dto.Storage,
            Battery = dto.Battery,
            Camera = dto.Camera,
            Display = dto.Display,
            Chipset = dto.Chipset,
            Os = dto.Os,
            Color = dto.Color,
            Network = dto.Network
        };

        private static ProductDto MapToDto(Product product) => new()
        {
            Id = product.Id,
            Name = product.Name,
            Brand = product.Brand.ToString(),
            Model = product.Model,
            Condition = product.Condition == ProductCondition.ExUk ? "Ex-UK" : product.Condition.ToString(),
            Category = product.Category.ToString(),
            Price = product.Price,
            CompareAtPrice = product.CompareAtPrice,
            StockQuantity = product.StockQuantity,
            Images = product.Images,
            Specs = new ProductSpecsDto
            {
                Ram = product.Specs.Ram,
                Storage = product.Specs.Storage,
                Battery = product.Specs.Battery,
                Camera = product.Specs.Camera,
                Display = product.Specs.Display,
                Chipset = product.Specs.Chipset,
                Os = product.Specs.Os,
                Color = product.Specs.Color,
                Network = product.Specs.Network
            },
            Description = product.Description,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            UnitsSold = product.UnitsSold,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
