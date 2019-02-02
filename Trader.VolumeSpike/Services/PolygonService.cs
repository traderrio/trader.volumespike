﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;
using NATS.Client;
using Newtonsoft.Json;
using Trader.Common.Enums;
using Trader.Common.Extensions;
using Trader.Domain;
using Trader.Domain.Services;
using Trader.Services.TradingActivityHelpers;
using Trader.VolumeSpike.Common.Configuration;
using Trader.VolumeSpike.Infrastructure.JsonConverters;
using Trader.VolumeSpike.Services.Interfaces;

namespace Trader.VolumeSpike.Services
{
	public class PolygonService : IPolygonService, IDisposable
	{
		private readonly IOptions<AppSettings> _appSettings;
		private NATS.Client.Options _opts;

		private IConnection _polygonConnection;
		private IAsyncSubscription _stockLastTradeSubscription;
		public List<StockLastTrade> StockLastTrades;
		private ISymbolService _symbolService;
		private ILastTradesService _lastTradesService;
		private IVolumeRecordService _volumeRecordService;
		public List<SymbolDetails> ValidSymbols { get; set; }

		public PolygonService(IOptions<AppSettings> appSettings, ISymbolService symbolService, ILastTradesService lastTradesService, IVolumeRecordService volumeRecordService)
		{
			_appSettings = appSettings;
			SetupOptions();
			_symbolService = symbolService;
			_lastTradesService = lastTradesService;
			_volumeRecordService = volumeRecordService;
			StockLastTrades = new List<StockLastTrade>(_appSettings.Value.DataProcessing.IntraDayBulkCount);
			ValidSymbols = _symbolService.GetValidSymbols();
		}

		public void SubscribeToTrades()
		{
			Serilog.Log.Warning("SubscribeToTrades");

			EnsureConnectionExists();
			_stockLastTradeSubscription = _polygonConnection.SubscribeAsync("T.*", StockTradeHandlerAsync);
		}

		private void SetupOptions()
		{
			_opts = ConnectionFactory.GetDefaultOptions();
			_opts.Servers = _appSettings.Value.Polygon.StockServers;
			_opts.Token = _appSettings.Value.Polygon.ApiKey;
		}

		private void StockTradeHandlerAsync(object sender, MsgHandlerEventArgs e)
		{
			if (TryGetMessageAsString(e, out var message))
			{
				return;
			}

			StockLastTrade lastTrade = Deserialize<StockLastTrade>(message, new PolygonStockTradeConverter());

			if (string.IsNullOrWhiteSpace(lastTrade?.Ticker) || lastTrade.Price == 0 || lastTrade.Size == 0)
			{
				return;
			}


			if (lastTrade.Price < 1 || lastTrade.Price > 2000)
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

			var timeFrame = 15;
			var length = 10;
			var ratioPercentage = 50;
			var toDate = DateTime.Now;
			var fromDate = toDate.ResolveFromDate(timeFrame);

			var openMarketStartTime = new DateTime(toDate.Year, toDate.Month, toDate.Day, toDate.Hour, 30, 0);

			List<StockLastTrade> lastTrades = _lastTradesService.GetLastTrades(lastTrade.Ticker, fromDate, toDate, length, timeFrame);

			if (!lastTrades.Any()) return;

			var groups = lastTrades.GroupBy(x =>
				{
					var stamp = x.DateTime;
					stamp = stamp.AddMinutes(-(stamp.Minute % timeFrame));
					stamp = stamp.AddMilliseconds(-stamp.Millisecond - 1000 * stamp.Second);
					return stamp;
				})
				.Select(g => new OneMinuteDataGroup { TimeStamp = g.Key, LastTrades = g })
				.Take(length).ToList();

			var groupedRecords = groups.Select(x => new VolumeRecord
			{
				Date = x.TimeStamp,
				Price = (double)x.LastTrades.First().Price,
				Volume = x.LastTrades.Sum(r => r.Size),
				VolumeFriendly = x.LastTrades.Sum(r => (double)r.Size).HumanReadable(4),
				TimeFrame = timeFrame.ToString(),
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
				r.TimeFrame = timeFrame.ToString();
				r.AverageVolume = groupedRecords.Any(t => t.Date != x.Date)
					? groupedRecords.Where(t => t.Date != x.Date).Average(a => a.Volume).HumanReadable(4)
					: "N/A";

				records.Add(r);
			}

			if (!records.Any()) return;

			var avgRatio = records.Average(x => x.Ratio);
			var spikeRatio = avgRatio + (avgRatio * ratioPercentage / 100);

			if (records[0].Ratio >= spikeRatio)
			{
				var volumeSpike = new Trader.Domain.VolumeSpike
				{
					Ticker = lastTrade.Ticker,
					Date = toDate,
					MinuteTimeFrame = timeFrame,
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

		private bool TryGetMessageAsString(MsgHandlerEventArgs e, out string message)
		{
			if (e.Message.Data == null)
			{
				message = string.Empty;
				return false;
			}

			message = Encoding.UTF8.GetString(e.Message.Data);
			return string.IsNullOrWhiteSpace(message);
		}


		private T Deserialize<T>(string message, JsonConverter converter)
		{
			T prices;
			using (var sr = new StringReader(message))
			{
				using (var jr = new JsonTextReader(sr))
				{
					prices = (T)converter.ReadJson(jr, typeof(T), null, null);
				}
			}

			return prices;
		}

		private void EnsureConnectionExists()
		{
			if (_polygonConnection == null || _polygonConnection.IsClosed())
			{
				_polygonConnection = new ConnectionFactory().CreateConnection(_opts);
			}
		}

		public void Dispose()
		{
			_polygonConnection?.Dispose();
			_stockLastTradeSubscription?.Dispose();
		}
	}
}