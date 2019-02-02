using System;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Trader.Domain;
using Trader.VolumeSpike.Common.Configuration;
using Trader.VolumeSpike.Infrastructure.DbContext.Interfaces;

namespace Trader.VolumeSpike.Infrastructure.DbContext
{
	public class BaseDbContext : IBaseDbContext
	{
		private readonly IMongoDatabase _database;
		private readonly IOptions<AppSettings> _appSettings;

		protected BaseDbContext(IMongoDatabase database, IOptions<AppSettings> appSettings)
		{
			_database = database;
			_appSettings = appSettings;
		}

		public IMongoCollection<T> GetCollection<T>() where T : Entity, new()
		{
			return _database.GetCollection<T>(new T().CollectionName);
		}

		public IMongoCollection<T> GetCollection<T>(Type getType) where T : IEntity
		{
			var collName = ((IEntity)(Activator.CreateInstance(getType))).CollectionName;
			return _database.GetCollection<T>(collName);
		}

		public IMongoCollection<BsonDocument> GetAsBsonCollection(string name)
		{
			return _database.GetCollection<BsonDocument>(name);
		}
	}
}