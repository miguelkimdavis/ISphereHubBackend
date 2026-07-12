using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Domain.Entities
{
    public class OrderItem
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;

        public static OrderItem Create(string productId, string productName, string? imageUrl, int quantity, decimal unitPrice)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

            if (unitPrice < 0)
                throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

            return new OrderItem
            {
                ProductId = productId,
                ProductName = productName,
                ImageUrl = imageUrl,
                Quantity = quantity,
                UnitPrice = unitPrice
            };
        }
    }
}
