using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Trader.Common.Extensions;
using Trader.VolumeSpike.Common.Configuration;
using Trader.VolumeSpike.Domain;
using Trader.VolumeSpike.Infrastructure;
using Trader.VolumeSpike.Services.Interfaces;

namespace Trader.VolumeSpike.Services
{
	public class MongoPolygonDataSaver : ISpecificPolygonDataSaver
	{
		private readonly IOptions<AppSettings> _appSettings;
		private NATS.Client.Options _opts;
		private readonly IMongoCollection<StockLastTrade> _stockLastTradesCollection;

		public MongoPolygonDataSaver(IOptions<AppSettings> appSettings, IMongoDbContext mongoDbContext)
		{
			_appSettings = appSettings;
			_stockLastTradesCollection = mongoDbContext.GetCollection<StockLastTrade>();
		}

		public async Task SaveLastTradeDataAsync(StockLastTrade lastTrade)
		{
			await _stockLastTradesCollection.InsertOneAsync(lastTrade);
		}

		public async Task SaveBulkLastTradeDataAsync(List<StockLastTrade> stockLastTrades)
		{
			await _stockLastTradesCollection.InsertManyAsync(stockLastTrades);
		}

		public void SaveLastTradeData(StockLastTrade lastTrade)
		{
			_stockLastTradesCollection.InsertOne(lastTrade);
		}

		public void SaveBulkLastTradeData(List<StockLastTrade> stockLastTrades)
		{
			_stockLastTradesCollection.InsertMany(stockLastTrades);
		}

		public async Task<StockLastTrade> LastTradeRecord(string ticker)
		{
			var sort = Builders<StockLastTrade>.Sort.Descending(x => x.DateTime);

			return await _stockLastTradesCollection.Find(x => x.Ticker == ticker).Sort(sort).FirstOrDefaultAsync();
		}
	}
}