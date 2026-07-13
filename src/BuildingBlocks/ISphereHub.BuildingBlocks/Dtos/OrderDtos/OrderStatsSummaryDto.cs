using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.BuildingBlocks.Dtos.OrderDtos
{
    public class OrderStatsSummaryDto
    {
        public long TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public long OnlineOrders { get; set; }
        public long InStoreOrders { get; set; }
        public decimal OnlineRevenue { get; set; }
        public decimal InStoreRevenue { get; set; }
    }
}
