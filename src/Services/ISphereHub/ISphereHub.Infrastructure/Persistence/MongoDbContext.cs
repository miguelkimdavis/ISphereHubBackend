using ISphereHub.Domain.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Infrastructure.Persistence
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly MongoDbSettings _settings;

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            _settings = settings.Value;
            var client = new MongoClient(_settings.ConnectionString);
            _database = client.GetDatabase(_settings.DatabaseName);
        }

        public IMongoCollection<Product> Products => _database.GetCollection<Product>(_settings.ProductsCollectionName);
        public IMongoCollection<Order> Orders => _database.GetCollection<Order>(_settings.OrdersCollectionName);
        public IMongoCollection<AdminUser> AdminUsers => _database.GetCollection<AdminUser>(_settings.AdminUsersCollectionName);

        public async Task EnsureIndexesAsync()
        {
            var productIndexes = new List<CreateIndexModel<Product>>
            {
                new(Builders<Product>.IndexKeys.Ascending(p => p.Brand)),
                new(Builders<Product>.IndexKeys.Ascending(p => p.Condition)),
                new(Builders<Product>.IndexKeys.Ascending(p => p.Category)),
                new(Builders<Product>.IndexKeys.Ascending(p => p.Price)),
                new(Builders<Product>.IndexKeys.Descending(p => p.CreatedAt)),
                new(Builders<Product>.IndexKeys.Ascending(p => p.IsActive)),
                new(Builders<Product>.IndexKeys.Text(p => p.Name))
            };
            await Products.Indexes.CreateManyAsync(productIndexes);

            var orderIndexes = new List<CreateIndexModel<Order>>
            {
                new(Builders<Order>.IndexKeys.Descending(o => o.CreatedAt)),
                new(Builders<Order>.IndexKeys.Ascending(o => o.Status)),
                new(Builders<Order>.IndexKeys.Ascending(o => o.Channel)),
                new(Builders<Order>.IndexKeys.Ascending(o => o.OrderNumber), new CreateIndexOptions { Unique = true }),
                new(Builders<Order>.IndexKeys.Ascending(o => o.PhoneNumber))
            };
            await Orders.Indexes.CreateManyAsync(orderIndexes);

            var adminIndexes = new List<CreateIndexModel<AdminUser>>
            {
                new(Builders<AdminUser>.IndexKeys.Ascending(a => a.Username), new CreateIndexOptions { Unique = true })
            };
            await AdminUsers.Indexes.CreateManyAsync(adminIndexes);
        }
    }
}
