using ISphereHub.Application.Interfaces;
using ISphereHub.BuildingBlocks.Dtos.AdminDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISphereHub.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AdminLoginResponseDto>> Login([FromBody] AdminLoginDto dto, CancellationToken ct)
        {
            var result = await _adminService.LoginAsync(dto, ct);
            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            return Ok(new
            {
                username = User.Identity?.Name,
                role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
            });
        }
    }
}
