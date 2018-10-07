using System;
using MongoDB.Bson;
using MongoDB.Driver;
using Trader.VolumeSpike.Domain;

namespace Trader.VolumeSpike.Infrastructure
{
    public class MongoDbContext : IMongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IMongoDatabase database)
        {
            _database = database;
        }

        public IMongoCollection<T> GetCollection<T>() where T : Entity, new()
        {
            return _database.GetCollection<T>(new T().CollectionName);
        }

        public IMongoCollection<T> GetCollection<T>(Type getType) where T : IEntity
        {
            var collName = ((IEntity) (Activator.CreateInstance(getType))).CollectionName;
            return _database.GetCollection<T>(collName);
        }

        public IMongoCollection<BsonDocument> GetAsBsonCollection(string name)
        {
            return _database.GetCollection<BsonDocument>(name);
        }
    }
}