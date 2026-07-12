using ISphereHub.Domain.Common;
using ISphereHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Domain.Entities
{
    public class Order : BaseEntity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Address { get; set; }
        public List<OrderItem> Items { get; set; } = new();

        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public OrderChannel Channel { get; set; } = OrderChannel.Online;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.PayOnDelivery;
        public string? ServedByAdminId { get; set; }
        public string? ServedByAdminUsername { get; set; }
        public string? Notes { get; set; }

        public static Order CreateOnlineOrder(
            string customerName,
            string phoneNumber,
            string location,
            string address,
            List<OrderItem> items,
            string? notes = null)
        {
            ValidateCommon(customerName, phoneNumber, items);

            if (string.IsNullOrWhiteSpace(location))
                throw new ArgumentException("Location is required for delivery orders.", nameof(location));

            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address is required for delivery orders.", nameof(address));

            return new Order
            {
                OrderNumber = GenerateOrderNumber(),
                CustomerName = customerName.Trim(),
                PhoneNumber = phoneNumber.Trim(),
                Location = location.Trim(),
                Address = address.Trim(),
                Items = items,
                TotalAmount = items.Sum(i => i.LineTotal),
                Status = OrderStatus.Pending,
                Channel = OrderChannel.Online,
                PaymentMethod = PaymentMethod.PayOnDelivery,
                Notes = notes
            };
        }

        public static Order CreateInStoreSale(
            string customerName,
            string phoneNumber,
            List<OrderItem> items,
            PaymentMethod paymentMethod,
            string servedByAdminId,
            string servedByAdminUsername,
            string? notes = null)
        {
            ValidateCommon(customerName, phoneNumber, items);

            if (paymentMethod == PaymentMethod.PayOnDelivery)
                throw new ArgumentException("In-store sales cannot use pay-on-delivery as a payment method.", nameof(paymentMethod));

            return new Order
            {
                OrderNumber = GenerateOrderNumber(),
                CustomerName = customerName.Trim(),
                PhoneNumber = phoneNumber.Trim(),
                Location = "In-Store",
                Address = "ISphere Hub Shop",
                Items = items,
                TotalAmount = items.Sum(i => i.LineTotal),
                Status = OrderStatus.Delivered,
                Channel = OrderChannel.InStore,
                PaymentMethod = paymentMethod,
                ServedByAdminId = servedByAdminId,
                ServedByAdminUsername = servedByAdminUsername,
                Notes = notes
            };
        }

        private static void ValidateCommon(string customerName, string phoneNumber, List<OrderItem> items)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                throw new ArgumentException("Customer name is required.", nameof(customerName));

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Phone number is required.", nameof(phoneNumber));

            if (items is null || items.Count == 0)
                throw new ArgumentException("An order must contain at least one item.", nameof(items));
        }

        private static string GenerateOrderNumber()
        {
            return $"ISH-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        }

        public void UpdateStatus(OrderStatus status)
        {
            Status = status;
            Touch();
        }
    }
}
