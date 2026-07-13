using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.BuildingBlocks.Dtos.OrderDtos
{
    public class CreateInStoreSaleDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "CashInStore";
        public string? Notes { get; set; }
        public List<OrderItemRequestDto> Items { get; set; } = new();
    }
}
