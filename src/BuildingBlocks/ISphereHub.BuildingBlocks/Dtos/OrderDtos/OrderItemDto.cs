using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.BuildingBlocks.Dtos.OrderDtos
{
    public class OrderItemDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
