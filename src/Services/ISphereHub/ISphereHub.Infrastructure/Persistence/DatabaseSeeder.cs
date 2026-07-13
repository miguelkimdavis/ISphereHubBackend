using ISphereHub.Application.Interfaces;
using ISphereHub.Domain.Entities;
using ISphereHub.Domain.Enums;
using ISphereHub.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Infrastructure.Persistence
{
    public class DatabaseSeeder
    {
        private readonly MongoDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(
            MongoDbContext context,
            IPasswordHasher passwordHasher,
            IConfiguration configuration,
            ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            await _context.EnsureIndexesAsync();

            var anyAdmin = await _context.AdminUsers.Find(MongoDB.Driver.FilterDefinition<AdminUser>.Empty).AnyAsync();
            if (!anyAdmin)
            {
                var defaultUsername = _configuration["SeedAdmin:Username"] ?? "admin";
                var defaultPassword = _configuration["SeedAdmin:Password"] ?? "ChangeMe123!";

                var admin = AdminUser.Create(defaultUsername, _passwordHasher.Hash(defaultPassword), "Admin");
                await _context.AdminUsers.InsertOneAsync(admin);

                _logger.LogWarning(
                    "Seeded default admin account '{Username}'. CHANGE THIS PASSWORD IMMEDIATELY in production.",
                    defaultUsername);
            }

            var anyProduct = await _context.Products.Find(MongoDB.Driver.FilterDefinition<Product>.Empty).AnyAsync();
            if (!anyProduct)
            {
                var sampleProducts = BuildSampleProducts();
                await _context.Products.InsertManyAsync(sampleProducts);
                _logger.LogInformation("Seeded {Count} sample products.", sampleProducts.Count);
            }
        }

        private static List<Product> BuildSampleProducts()
        {
            return new List<Product>
            {
                Product.Create(
                    "iPhone 13", ProductBrand.Apple, "iPhone 13", ProductCondition.Refurbished, ProductCategory.Phone,
                    75000, 12,
                    new List<string> { "https://images.unsplash.com/photo-1632661674596-df8be070a5c5?w=800" },
                    new ProductSpecs { Ram = "4GB", Storage = "128GB", Battery = "3240mAh", Camera = "12MP Dual", Display = "6.1\" OLED", Chipset = "A15 Bionic", Os = "iOS 17", Color = "Midnight" },
                    "Refurbished iPhone 13, UK-grade, fully tested with 6-month warranty.", 89000, true),

                Product.Create(
                    "iPhone 14 Pro Max", ProductBrand.Apple, "iPhone 14 Pro Max", ProductCondition.New, ProductCategory.Phone,
                    165000, 6,
                    new List<string> { "https://images.unsplash.com/photo-1678652197831-2d180705cd2c?w=800" },
                    new ProductSpecs { Ram = "6GB", Storage = "256GB", Battery = "4323mAh", Camera = "48MP Triple", Display = "6.7\" Super Retina XDR", Chipset = "A16 Bionic", Os = "iOS 17", Color = "Deep Purple" },
                    "Brand new, sealed in box, with Apple warranty.", null, true),

                Product.Create(
                    "iPhone 12", ProductBrand.Apple, "iPhone 12", ProductCondition.ExUk, ProductCategory.Phone,
                    58000, 15,
                    new List<string> { "https://images.unsplash.com/photo-1603921326210-6edd2d60ca68?w=800" },
                    new ProductSpecs { Ram = "4GB", Storage = "64GB", Battery = "2815mAh", Camera = "12MP Dual", Display = "6.1\" OLED", Chipset = "A14 Bionic", Os = "iOS 16", Color = "Black" },
                    "Ex-UK unit, minor cosmetic wear, fully functional.", 65000, false),

                Product.Create(
                    "Samsung Galaxy S23 Ultra", ProductBrand.Samsung, "Galaxy S23 Ultra", ProductCondition.New, ProductCategory.Phone,
                    145000, 8,
                    new List<string> { "https://images.unsplash.com/photo-1676315163438-9b7ea6f4b3d1?w=800" },
                    new ProductSpecs { Ram = "12GB", Storage = "256GB", Battery = "5000mAh", Camera = "200MP Quad", Display = "6.8\" Dynamic AMOLED", Chipset = "Snapdragon 8 Gen 2", Os = "Android 14", Color = "Phantom Black" },
                    "Flagship Samsung, brand new, sealed box.", null, true),

                Product.Create(
                    "Samsung Galaxy A54", ProductBrand.Samsung, "Galaxy A54", ProductCondition.New, ProductCategory.Phone,
                    42000, 20,
                    new List<string> { "https://images.unsplash.com/photo-1610945415295-d9bbf067e59c?w=800" },
                    new ProductSpecs { Ram = "8GB", Storage = "128GB", Battery = "5000mAh", Camera = "50MP Triple", Display = "6.4\" Super AMOLED", Chipset = "Exynos 1380", Os = "Android 14", Color = "Awesome Lime" },
                    "Great budget-friendly all-rounder, brand new.", null, false),

                Product.Create(
                    "Samsung Galaxy S21", ProductBrand.Samsung, "Galaxy S21", ProductCondition.Refurbished, ProductCategory.Phone,
                    38000, 10,
                    new List<string> { "https://images.unsplash.com/photo-1610792516307-ea5acd9c3b00?w=800" },
                    new ProductSpecs { Ram = "8GB", Storage = "128GB", Battery = "4000mAh", Camera = "64MP Triple", Display = "6.2\" Dynamic AMOLED", Chipset = "Exynos 2100", Os = "Android 13", Color = "Phantom Gray" },
                    "Refurbished, tested battery health above 85%.", 45000, false),

                Product.Create(
                    "20W USB-C Fast Charger", ProductBrand.Apple, "Universal", ProductCondition.New, ProductCategory.Accessory,
                    2500, 50,
                    new List<string> { "https://images.unsplash.com/photo-1583863788434-e58a36330cf0?w=800" },
                    new ProductSpecs { Network = "PD Fast Charging" },
                    "Compatible with iPhone 8 and newer.", null, false),

                Product.Create(
                    "Clear Silicone Case", ProductBrand.Other, "Universal", ProductCondition.New, ProductCategory.Accessory,
                    800, 100,
                    new List<string> { "https://images.unsplash.com/photo-1601784551446-20c9e07cdbdb?w=800" },
                    new ProductSpecs(),
                    "Slim protective case, multiple sizes available.", null, false),
            };
        }
    }
}
