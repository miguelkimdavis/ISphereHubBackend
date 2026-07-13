using ISphereHub.BuildingBlocks.Dtos.ProductDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Application.Interfaces
{
    public interface IProductService
    {
        Task<ProductListResponse> GetAllProductsAsync(ProductQueryDto query, CancellationToken ct = default);
        Task<ProductDto> GetProductByIdAsync(string id, CancellationToken ct = default);
        Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken ct = default);
        Task<ProductDto> UpdateProductAsync(string id, UpdateProductDto dto, CancellationToken ct = default);
        Task DeleteProductAsync(string id, CancellationToken ct = default);
        Task<ProductDto> AdjustStockAsync(string id, AdjustStockDto dto, CancellationToken ct = default);
        Task<IReadOnlyList<string>> GetBrandsAsync(CancellationToken ct = default);
    }
}
