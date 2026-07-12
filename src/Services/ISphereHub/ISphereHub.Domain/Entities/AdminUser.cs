using ISphereHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Domain.Entities
{
    public class AdminUser : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Admin"; // Admin 
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }

        public static AdminUser Create(string username, string passwordHash, string role = "Admin")
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username is required.", nameof(username));

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash is required.", nameof(passwordHash));

            return new AdminUser
            {
                Username = username.Trim().ToLowerInvariant(),
                PasswordHash = passwordHash,
                Role = role
            };
        }
    }
}
