using System.Collections.Generic;
using System.Threading.Tasks;
using Trader.VolumeSpike.Domain;
using Trader.VolumeSpike.Models;

namespace Trader.VolumeSpike.Services.Interfaces
{
	public interface IPolygonDataSaver
	{
		Task SaveLastTradeDataAsync(StockLastTrade lastTrade);
		Task SaveBulkLastTradeDataAsync(List<StockLastTrade> lastTrades);
		void SaveLastTradeData(StockLastTrade lastTrade);
		void SaveBulkLastTradeData(List<StockLastTrade> lastTrades);
		Task<StockLastTrade> LastTradeRecord(string ticker);
		Task SaveAggregatedDataAsync(StockAggregatedModel stockAggregated);
		Task SaveQuoteDataAsync(StockQuoteModel stockQuote);
	}
}