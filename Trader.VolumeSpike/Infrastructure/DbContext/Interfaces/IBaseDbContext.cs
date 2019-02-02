using System;
using MongoDB.Bson;
using MongoDB.Driver;
using Trader.Domain;

namespace Trader.VolumeSpike.Infrastructure.DbContext.Interfaces
{
	public interface IBaseDbContext
	{
		IMongoCollection<T> GetCollection<T>() where T : Entity, new();
		IMongoCollection<T> GetCollection<T>(Type getType) where T : IEntity;
		IMongoCollection<BsonDocument> GetAsBsonCollection(string name);
	}
}