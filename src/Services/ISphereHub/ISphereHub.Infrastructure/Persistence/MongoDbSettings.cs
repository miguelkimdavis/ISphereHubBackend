using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Infrastructure.Persistence
{
    public class MongoDbSettings
    {
        public const string SectionName = "MongoDbSettings";

        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string ProductsCollectionName { get; set; } = "Products";
        public string OrdersCollectionName { get; set; } = "Orders";
        public string AdminUsersCollectionName { get; set; } = "AdminUsers";
    }
}
