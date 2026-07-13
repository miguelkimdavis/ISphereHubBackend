using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.BuildingBlocks.Dtos.ProductDtos
{
    public class AdjustStockDto
    {
        public int Delta { get; set; }
        public string? Reason { get; set; }
    }
}
