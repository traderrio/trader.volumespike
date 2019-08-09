using System;
using System.Collections.Generic;
using System.Linq;
using Easy.MessageHub;
using Microsoft.Extensions.Options;
using Trader.Common.Enums;
using Trader.Common.Extensions;
using Trader.Domain;
using Trader.Domain.Services;
using Trader.Services.TradingActivityHelpers;
using Trader.VolumeSpike.Common.Configuration;
using Trader.VolumeSpike.Infrastructure;
using Trader.VolumeSpike.Services.Interfaces;

namespace Trader.VolumeSpike.Services
{
	public interface IVolumeSpikesDetector
	{
	}

	public class VolumeSpikesDetector : IVolumeSpikesDetector
	{
	    private readonly IOptions<AppSettings> _appSettings;
        private readonly ISymbolService _symbolService;
        private readonly ILastTradesService _lastTradesService;
        private readonly IVolumeRecordService _volumeRecordService;

        private IList<SymbolDetails> ValidSymbols { get; set; }

        public VolumeSpikesDetector(IMessageHub messageHub,
	        IOptions<AppSettings> appSettings,
	        ISymbolService symbolService, 
	        ILastTradesService lastTradesService, 
	        IVolumeRecordService volumeRecordService)
        {
	        _appSettings = appSettings;
	        _symbolService = symbolService;
	        _lastTradesService = lastTradesService;
	        _volumeRecordService = volumeRecordService;

	        LoadSymbols();
	        
	        messageHub.Subscribe<IList<StreamingMessage>>(HandleMessages);
        }
        
        private void LoadSymbols()
        {
	        ValidSymbols = _symbolService.GetValidSymbols();
        }

        private void HandleMessages(IList<StreamingMessage> messages)
        {
	        foreach (var message in messages)
	        {
		        if (message is StockLastTradeMessage tradeMessage)
		        {
			        Detect(tradeMessage);
		        }
	        }
        }
        
        private void Detect(StockLastTradeMessage lastTrade)
        {
	        if (string.IsNullOrWhiteSpace(lastTrade?.Ticker) || lastTrade.Price == 0 || lastTrade.Size == 0)
			{
				return;
			}

			if (lastTrade.Price < _appSettings.Value.DataProcessing.MinimumPrice && lastTrade.Price > _appSettings.Value.DataProcessing.MaximumPrice)
			{
				return;
			}

			if (ValidSymbols.All(x => x.Symbol != lastTrade.Ticker))
			{
				return;
			}

			var toDate = DateTime.Now;
			var fromDate = toDate.ResolveFromDate(_appSettings.Value.DataProcessing.TimeFrame);

			var openMarketStartTime = new DateTime(toDate.Year, toDate.Month, toDate.Day, toDate.Hour, 30, 0);

			List<StockLastTrade> lastTrades = _lastTradesService.GetLastTrades(lastTrade.Ticker, fromDate, toDate, _appSettings.Value.DataProcessing.RecordLength, _appSettings.Value.DataProcessing.TimeFrame);

			if (!lastTrades.Any()) return;

			var groups = lastTrades.GroupBy(x =>
				{
					var stamp = x.DateTime;
					stamp = stamp.AddMinutes(-(stamp.Minute % _appSettings.Value.DataProcessing.TimeFrame));
					stamp = stamp.AddMilliseconds(-stamp.Millisecond - 1000 * stamp.Second);
					return stamp;
				})
				.Select(g => new OneMinuteDataGroup { TimeStamp = g.Key, LastTrades = g })
				.Take(_appSettings.Value.DataProcessing.RecordLength).ToList();

			var groupedRecords = groups.Select(x => new VolumeRecord
			{
				Date = x.TimeStamp,
				Price = (double)x.LastTrades.First().Price,
				Volume = x.LastTrades.Sum(r => r.Size),
				VolumeFriendly = x.LastTrades.Sum(r => (double)r.Size).HumanReadable(4),
				TimeFrame = _appSettings.Value.DataProcessing.TimeFrame.ToString(),
				TrendType = TrendType.Neutral,
			}).ToList();


			if (!groupedRecords.Any()) return;

			var records = new List<VolumeRecord>();
			foreach (var x in groupedRecords.Where(x => x.Date != openMarketStartTime).ToList())
			{
				var r = new VolumeRecord();
				r.Date = x.Date;
				r.Price = x.Price;
				r.Volume = x.Volume;
				r.Ticker = lastTrade.Ticker;
				r.VolumeFriendly = x.VolumeFriendly;
				r.TrendType = x.ResolveTrendType(groupedRecords);
				r.Ratio = x.CalculateRatio(groupedRecords);
				r.TimeFrame = _appSettings.Value.DataProcessing.TimeFrame.ToString();
				r.AverageVolume = groupedRecords.Any(t => t.Date != x.Date)
					? groupedRecords.Where(t => t.Date != x.Date).Average(a => a.Volume).HumanReadable(4)
					: "N/A";

				records.Add(r);
			}

			if (!records.Any()) return;

			var avgRatio = records.Average(x => x.Ratio);
			var spikeRatio = avgRatio + (avgRatio * _appSettings.Value.DataProcessing.RatioPercentage / 100);

			if (records[0].Ratio >= spikeRatio)
			{
				var volumeSpike = new Trader.Domain.VolumeSpike
				{
					Ticker = lastTrade.Ticker,
					Date = toDate,
					MinuteTimeFrame = _appSettings.Value.DataProcessing.TimeFrame,
					VolumeRecords = records
				};

				if (volumeSpike.VolumeRecords.Any())
				{
					VolumeRecord volumeRecord = volumeSpike.VolumeRecords[0];
					volumeRecord.Date = DateTime.Now;
					volumeRecord.ApiType = ApiType.Polygon;

					var symbol = ValidSymbols.FirstOrDefault(x => x.Symbol == volumeRecord.Ticker);

					if (symbol != null)
					{
						volumeRecord.Industry = symbol.Industry;
						volumeRecord.Sector = symbol.Sector;
					}

					switch (volumeRecord.TrendType)
					{
						case TrendType.Bullish:
							_volumeRecordService.SaveVolumeRecord(volumeRecord);
							break;
						case TrendType.Bearish:
							_volumeRecordService.SaveVolumeRecord(volumeRecord);
							break;
						case TrendType.None:
							break;
						case TrendType.Neutral:
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}
        }
    }
}