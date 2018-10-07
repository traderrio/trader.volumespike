using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Trader.VolumeSpike.Common;
using Trader.VolumeSpike.Common.Configuration;
using Trader.VolumeSpike.Domain;
using Trader.VolumeSpike.Models;
using Trader.VolumeSpike.Services.Interfaces;

namespace Trader.VolumeSpike.Services
{
    public class PolygonDataSaver : IPolygonDataSaver
    {
        private readonly IOptions<AppSettings> _options;
        private readonly IServiceProvider _serviceProvider;
        private ISpecificPolygonDataSaver _specificPolygonDataSaver;

        public PolygonDataSaver(IOptions<AppSettings> options, IServiceProvider serviceProvider)
        {
            _options = options;
            _serviceProvider = serviceProvider;
            ResolveSpecificDataSaver();
        }

        private void ResolveSpecificDataSaver()
        {
            var saverType = _options.Value.Polygon.SaverType;
            switch (saverType)
            {
                case PolygonDataSaverType.Mongo:
                    _specificPolygonDataSaver = _serviceProvider.GetRequiredService<MongoPolygonDataSaver>();
                    break;
                case PolygonDataSaverType.Redis:
                    _specificPolygonDataSaver = _serviceProvider.GetRequiredService<RedisPolygonDataSaver>();
                    break;
                default: throw new InvalidOperationException($"Saver type is not supported: {saverType}");
            }
        }

        public async Task SaveLastTradeDataAsync(StockLastTrade lastTrade)
        {
	        await _specificPolygonDataSaver.SaveLastTradeDataAsync(lastTrade);
        }


	    public async Task SaveBulkLastTradeDataAsync(List<StockLastTrade> lastTrades)
	    {
			await _specificPolygonDataSaver.SaveBulkLastTradeDataAsync(lastTrades);
		}

	    public void SaveLastTradeData(StockLastTrade lastTrade)
	    {
			_specificPolygonDataSaver.SaveLastTradeData(lastTrade);
		}

	    public void SaveBulkLastTradeData(List<StockLastTrade> lastTrades)
	    {
			_specificPolygonDataSaver.SaveBulkLastTradeData(lastTrades);
		}
	    public async Task<StockLastTrade> LastTradeRecord(string ticker)
	    {
		    return await _specificPolygonDataSaver.LastTradeRecord(ticker);
	    }

		public Task SaveAggregatedDataAsync(StockAggregatedModel stockAggregated)
        {
            throw new NotImplementedException();
        }

        public Task SaveQuoteDataAsync(StockQuoteModel stockQuote)
        {
            throw new NotImplementedException();
        }
    }
}