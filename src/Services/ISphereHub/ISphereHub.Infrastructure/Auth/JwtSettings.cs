using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Infrastructure.Auth
{
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = "ISphereHub";
        public string Audience { get; set; } = "ISphereHubClients";
        public int ExpiryMinutes { get; set; } = 480;
    }
}
