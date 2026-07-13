using ISphereHub.Application.Interfaces;
using ISphereHub.BuildingBlocks.Dtos.ProductDtos;
using Microsoft.AspNetCore.Mvc;

namespace ISphereHub.Api.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<ProductListResponse>> GetAll([FromQuery] ProductQueryDto query, CancellationToken ct)
        {
            var result = await _productService.GetAllProductsAsync(query, ct);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetById(string id, CancellationToken ct)
        {
            var result = await _productService.GetProductByIdAsync(id, ct);
            return Ok(result);
        }

        [HttpGet("brands")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetBrands(CancellationToken ct)
        {
            var result = await _productService.GetBrandsAsync(ct);
            return Ok(result);
        }
    }
}
