using ISphereHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Domain.Interfaces
{
    public interface IAdminRepository
    {
        Task<AdminUser?> GetByUsernameAsync(string username, CancellationToken ct = default);
        Task<AdminUser?> GetByIdAsync(string id, CancellationToken ct = default);
        Task CreateAsync(AdminUser adminUser, CancellationToken ct = default);
        Task<bool> UpdateLastLoginAsync(string id, CancellationToken ct = default);
        Task<bool> AnyAdminExistsAsync(CancellationToken ct = default);
    }
}
