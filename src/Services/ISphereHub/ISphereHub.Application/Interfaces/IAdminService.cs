using ISphereHub.BuildingBlocks.Dtos.AdminDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Application.Interfaces
{
    public interface IAdminService
    {
        Task<AdminLoginResponseDto> LoginAsync(AdminLoginDto dto, CancellationToken ct = default);
    }
}
