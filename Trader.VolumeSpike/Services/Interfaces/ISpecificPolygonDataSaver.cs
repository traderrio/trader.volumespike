using System.Collections.Generic;
using System.Threading.Tasks;
using Trader.VolumeSpike.Domain;

namespace Trader.VolumeSpike.Services.Interfaces
{
	public interface ISpecificPolygonDataSaver
	{
		Task SaveLastTradeDataAsync(StockLastTrade lastTrade);
		Task SaveBulkLastTradeDataAsync(List<StockLastTrade> stockLastTrades);

		void SaveLastTradeData(StockLastTrade lastTrade);
		void SaveBulkLastTradeData(List<StockLastTrade> stockLastTrades);

		Task<StockLastTrade> LastTradeRecord(string ticker);
	}
}