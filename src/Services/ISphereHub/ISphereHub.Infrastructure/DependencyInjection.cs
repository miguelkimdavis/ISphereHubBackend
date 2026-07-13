using ISphereHub.Application.Interfaces;
using ISphereHub.Domain.Interfaces;
using ISphereHub.Infrastructure.Auth;
using ISphereHub.Infrastructure.Persistence;
using ISphereHub.Infrastructure.Repositories;
using ISphereHub.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            BsonConfig.Configure();

            services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

            services.AddSingleton<MongoDbContext>();

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IAdminRepository, AdminRepository>();

            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            services.AddScoped<IImageUploadService, LocalImageUploadService>();

            return services;
        }
    }
}
