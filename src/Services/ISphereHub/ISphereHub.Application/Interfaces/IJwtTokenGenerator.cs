using ISphereHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        (string Token, DateTime ExpiresAt) GenerateToken(AdminUser adminUser);
    }
}
