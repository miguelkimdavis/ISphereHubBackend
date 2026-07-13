using ISphereHub.Domain.Entities;
using ISphereHub.Domain.Interfaces;
using ISphereHub.Infrastructure.Persistence;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Infrastructure.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly IMongoCollection<AdminUser> _adminUsers;

        public AdminRepository(MongoDbContext context)
        {
            _adminUsers = context.AdminUsers;
        }

        public async Task<AdminUser?> GetByUsernameAsync(string username, CancellationToken ct = default)
        {
            return await _adminUsers.Find(a => a.Username == username).FirstOrDefaultAsync(ct);
        }

        public async Task<AdminUser?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _)) return null;
            return await _adminUsers.Find(a => a.Id == id).FirstOrDefaultAsync(ct);
        }

        public async Task CreateAsync(AdminUser adminUser, CancellationToken ct = default)
        {
            await _adminUsers.InsertOneAsync(adminUser, cancellationToken: ct);
        }

        public async Task<bool> UpdateLastLoginAsync(string id, CancellationToken ct = default)
        {
            var update = Builders<AdminUser>.Update.Set(a => a.LastLoginAt, DateTime.UtcNow);
            var result = await _adminUsers.UpdateOneAsync(a => a.Id == id, update, cancellationToken: ct);
            return result.MatchedCount > 0;
        }

        public async Task<bool> AnyAdminExistsAsync(CancellationToken ct = default)
        {
            return await _adminUsers.Find(FilterDefinition<AdminUser>.Empty).AnyAsync(ct);
        }
    }
}
