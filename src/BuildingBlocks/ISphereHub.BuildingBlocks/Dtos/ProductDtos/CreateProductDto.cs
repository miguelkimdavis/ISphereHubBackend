using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.BuildingBlocks.Dtos.ProductDtos
{
    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string Category { get; set; } = "Phone";
        public decimal Price { get; set; }
        public decimal? CompareAtPrice { get; set; }
        public int StockQuantity { get; set; }
        public List<string> Images { get; set; } = new();
        public ProductSpecsDto Specs { get; set; } = new();
        public string? Description { get; set; }
        public bool IsFeatured { get; set; }
    }
}
