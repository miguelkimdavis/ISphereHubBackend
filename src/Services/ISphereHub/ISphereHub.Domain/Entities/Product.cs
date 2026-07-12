using ISphereHub.Domain.Common;
using ISphereHub.Domain.Enums;
using ISphereHub.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ProductBrand Brand { get; set; }
        public string Model { get; set; } = string.Empty;
        public ProductCondition Condition { get; set; }
        public ProductCategory Category { get; set; } = ProductCategory.Phone;
        public decimal Price { get; set; }
        public decimal? CompareAtPrice { get; set; }
        public int StockQuantity { get; set; }

        public List<string> Images { get; set; } = new();
        public ProductSpecs Specs { get; set; } = new();
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public int UnitsSold { get; set; } = 0;

        public static Product Create(
            string name,
            ProductBrand brand,
            string model,
            ProductCondition condition,
            ProductCategory category,
            decimal price,
            int stockQuantity,
            List<string> images,
            ProductSpecs specs,
            string? description = null,
            decimal? compareAtPrice = null,
            bool isFeatured = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name is required.", nameof(name));

            if (price < 0)
                throw new ArgumentException("Price cannot be negative.", nameof(price));

            if (stockQuantity < 0)
                throw new ArgumentException("Stock quantity cannot be negative.", nameof(stockQuantity));

            return new Product
            {
                Name = name.Trim(),
                Brand = brand,
                Model = model.Trim(),
                Condition = condition,
                Category = category,
                Price = price,
                CompareAtPrice = compareAtPrice,
                StockQuantity = stockQuantity,
                Images = images ?? new List<string>(),
                Specs = specs ?? new ProductSpecs(),
                Description = description,
                IsFeatured = isFeatured
            };
        }

        public void UpdateDetails(
            string name,
            ProductBrand brand,
            string model,
            ProductCondition condition,
            ProductCategory category,
            decimal price,
            decimal? compareAtPrice,
            List<string> images,
            ProductSpecs specs,
            string? description,
            bool isFeatured,
            bool isActive)
        {
            Name = name.Trim();
            Brand = brand;
            Model = model.Trim();
            Condition = condition;
            Category = category;
            Price = price;
            CompareAtPrice = compareAtPrice;
            Images = images ?? new List<string>();
            Specs = specs ?? new ProductSpecs();
            Description = description;
            IsFeatured = isFeatured;
            IsActive = isActive;
            Touch();
        }

        public void AdjustStock(int delta)
        {
            var newQuantity = StockQuantity + delta;
            if (newQuantity < 0)
                throw new InvalidOperationException($"Insufficient stock for '{Name}'. Available: {StockQuantity}, requested: {-delta}.");

            StockQuantity = newQuantity;
            if (delta < 0)
                UnitsSold += -delta;

            Touch();
        }

        public bool HasSufficientStock(int quantity) => StockQuantity >= quantity;
    }
}
