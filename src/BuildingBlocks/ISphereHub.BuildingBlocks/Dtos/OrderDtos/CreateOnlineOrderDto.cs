using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.BuildingBlocks.Dtos.OrderDtos
{
    public class CreateOnlineOrderDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<OrderItemRequestDto> Items { get; set; } = new();
    }
}
