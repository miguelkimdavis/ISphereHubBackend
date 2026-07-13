using ISphereHub.Application.Interfaces;
using ISphereHub.BuildingBlocks.Dtos.AdminDtos;
using ISphereHub.BuildingBlocks.Exceptions;
using ISphereHub.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            IAdminRepository adminRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            ILogger<AdminService> logger)
        {
            _adminRepository = adminRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _logger = logger;
        }

        public async Task<AdminLoginResponseDto> LoginAsync(AdminLoginDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                throw new ValidationAppException("Username and password are required.");

            var admin = await _adminRepository.GetByUsernameAsync(dto.Username.Trim().ToLowerInvariant(), ct);

            if (admin is null || !admin.IsActive || !_passwordHasher.Verify(dto.Password, admin.PasswordHash))
            {
                _logger.LogWarning("Failed admin login attempt for username '{Username}'", dto.Username);
                throw new UnauthorizedAppException("Invalid username or password.");
            }

            var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(admin);
            await _adminRepository.UpdateLastLoginAsync(admin.Id, ct);

            _logger.LogInformation("Admin '{Username}' logged in successfully", admin.Username);

            return new AdminLoginResponseDto
            {
                Token = token,
                Username = admin.Username,
                Role = admin.Role,
                ExpiresAt = expiresAt
            };
        }
    }
}
