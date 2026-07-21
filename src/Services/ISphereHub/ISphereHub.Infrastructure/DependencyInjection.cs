using CloudinaryDotNet;
using ISphereHub.Application.Interfaces;
using ISphereHub.BuildingBlocks.Configurations;
using ISphereHub.Domain.Interfaces;
using ISphereHub.Infrastructure.Auth;
using ISphereHub.Infrastructure.Persistence;
using ISphereHub.Infrastructure.Repositories;
using ISphereHub.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            services.AddOptions<CloudinaryOptions>()
                .Bind(configuration.GetSection(CloudinaryOptions.SectionName))
                .ValidateOnStart(); 

            services.AddSingleton<MongoDbContext>();

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IAdminRepository, AdminRepository>();

            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<CloudinaryOptions>>().Value;
                var account = new Account(options.CloudName, options.ApiKey, options.ApiSecret);
                return new Cloudinary(account);
            });
            services.AddScoped<IImageUploadService, CloudinaryImageUploadService>();


            return services;
        }
    }
}
