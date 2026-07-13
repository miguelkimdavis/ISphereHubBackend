using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.BuildingBlocks.Dtos.ProductDtos
{
    public class UpdateProductDto : CreateProductDto
    {
        public bool IsActive { get; set; } = true;
    }
}
