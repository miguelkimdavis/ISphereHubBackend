using MongoDB.Bson.Serialization.Conventions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Infrastructure.Persistence
{
    public static class BsonConfig
    {
        private static bool _configured;
        private static readonly object Lock = new();

        public static void Configure()
        {
            if (_configured) return;

            lock (Lock)
            {
                if (_configured) return;

                var pack = new ConventionPack
                {
                    new CamelCaseElementNameConvention(),
                    new IgnoreExtraElementsConvention(true),
                    new EnumRepresentationConvention(MongoDB.Bson.BsonType.String)
                };
                ConventionRegistry.Register("ISphereHubConventions", pack, _ => true);

                _configured = true;
            }
        }
    }
}
