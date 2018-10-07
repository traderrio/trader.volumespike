using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis.Extensions.Core;
using Trader.VolumeSpike.Domain;
using Trader.VolumeSpike.Infrastructure;
using Trader.VolumeSpike.Services.Interfaces;

namespace Trader.VolumeSpike.Services
{
	public class RedisPolygonDataSaver : ISpecificPolygonDataSaver, IDisposable
	{
		private readonly ICacheClient _cacheClient;
		private readonly ILogger<PolygonDataSaver> _logger;

		public RedisPolygonDataSaver(ICacheClient cacheClient, ILogger<PolygonDataSaver> logger)
		{
			_cacheClient = cacheClient;
			_logger = logger;
		}

		public void Dispose()
		{
			_cacheClient.Dispose();
		}

		public Task SaveLastTradeDataAsync(StockLastTrade lastTrade)
		{
			try
			{
				var key = RedisKeyBuilder.GetLastTradesKey(lastTrade.Ticker);
				return _cacheClient.SortedSetAddAsync(key, lastTrade, lastTrade.DateTime.Ticks);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Cannot save data to redis");
			}
			return Task.CompletedTask;
		}

		public Task SaveBulkLastTradeDataAsync(List<StockLastTrade> stockLastTrades)
		{
			throw new NotImplementedException();
		}

		public void SaveLastTradeData(StockLastTrade lastTrade)
		{
			throw new NotImplementedException();
		}

		public void SaveBulkLastTradeData(List<StockLastTrade> stockLastTrades)
		{
			throw new NotImplementedException();
		}

		public Task<StockLastTrade> LastTradeRecord(string ticker)
		{
			throw new NotImplementedException();
		}
	}
}