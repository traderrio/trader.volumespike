using System;
using MongoDB.Bson;
using MongoDB.Driver;
using Trader.VolumeSpike.Domain;

namespace Trader.VolumeSpike.Infrastructure
{
	public interface IMongoDbContext
	{
		IMongoCollection<T> GetCollection<T>() where T : Entity, new();
		IMongoCollection<T> GetCollection<T>(Type getType) where T : IEntity;
		IMongoCollection<BsonDocument> GetAsBsonCollection(string name);
	}
}