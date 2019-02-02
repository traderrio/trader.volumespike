using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Trader.Domain;
using Trader.VolumeSpike.Infrastructure.DbContext.Interfaces;
using Trader.VolumeSpike.Services.Interfaces;

namespace Trader.VolumeSpike.Services
{
	public class LastTradesService : ILastTradesService
	{
		private readonly IMongoCollection<StockLastTrade> _stockLastTradeCollection;
		public LastTradesService(ILastTradesDbContext dbContext)
		{
			_stockLastTradeCollection = dbContext.GetCollection<StockLastTrade>();
		}

		public async Task<IList<StockLastTrade>> GetLastTrades(string ticker, DateTime from)
		{
			var stockLastTrades = await _stockLastTradeCollection.Find(x => x.Ticker == ticker & x.DateTime >= from)
				.ToListAsync();

			return stockLastTrades;
		}

		public List<StockLastTrade> GetLastTrades(string ticker, DateTime fromDate, DateTime to, int length, int timeFrame)
		{
			var minuteSort = Builders<StockLastTrade>.Sort.Descending(x => x.DateTime);
			var backMinutes = length * timeFrame;
			fromDate = fromDate.AddMinutes(-backMinutes);
			return _stockLastTradeCollection.Find(x => x.Ticker == ticker && x.DateTime >= fromDate && x.DateTime <= to).Sort(minuteSort).ToList();
		}
	}
}