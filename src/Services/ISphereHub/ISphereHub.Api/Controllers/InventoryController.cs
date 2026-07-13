using ISphereHub.Application.Interfaces;
using ISphereHub.BuildingBlocks.Dtos.ImageDtos;
using ISphereHub.BuildingBlocks.Dtos.ProductDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISphereHub.Api.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IImageUploadService _imageUploadService;

        public InventoryController(IProductService productService, IImageUploadService imageUploadService)
        {
            _productService = productService;
            _imageUploadService = imageUploadService;
        }

        [HttpGet]
        public async Task<ActionResult<ProductListResponse>> GetAll([FromQuery] ProductQueryDto query, CancellationToken ct)
        {
            query.IncludeInactive = true;
            var result = await _productService.GetAllProductsAsync(query, ct);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetById(string id, CancellationToken ct)
        {
            var result = await _productService.GetProductByIdAsync(id, ct);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto, CancellationToken ct)
        {
            var result = await _productService.CreateProductAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDto>> Update(string id, [FromBody] UpdateProductDto dto, CancellationToken ct)
        {
            var result = await _productService.UpdateProductAsync(id, dto, ct);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            await _productService.DeleteProductAsync(id, ct);
            return NoContent();
        }

        [HttpPatch("{id}/stock")]
        public async Task<ActionResult<ProductDto>> AdjustStock(string id, [FromBody] AdjustStockDto dto, CancellationToken ct)
        {
            var result = await _productService.AdjustStockAsync(id, dto, ct);
            return Ok(result);
        }

        [HttpPost("images")]
        [RequestSizeLimit(8 * 1024 * 1024)]
        public async Task<ActionResult<UploadImageResponseDto>> UploadImage(IFormFile file, CancellationToken ct)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new { success = false, message = "No file was uploaded." });

            await using var stream = file.OpenReadStream();
            var url = await _imageUploadService.SaveImageAsync(stream, file.FileName, file.ContentType, ct);

            return Ok(new UploadImageResponseDto { Url = url });
        }
    }
}
