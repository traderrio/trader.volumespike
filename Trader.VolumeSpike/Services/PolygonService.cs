using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;
using NATS.Client;
using Newtonsoft.Json;
using Serilog;
using Trader.Common.Extensions;
using Trader.VolumeSpike.Common;
using Trader.VolumeSpike.Common.Configuration;
using Trader.VolumeSpike.Domain;
using Trader.VolumeSpike.Infrastructure.JsonConverters;
using Trader.VolumeSpike.Models;
using Trader.VolumeSpike.Services.Interfaces;
using Options = NATS.Client.Options;

namespace Trader.VolumeSpike.Services
{
	public class PolygonService : IPolygonService, IDisposable
	{
		private readonly IOptions<AppSettings> _appSettings;
		private readonly IPolygonDataSaver _polygonDataSaver;
		private Options _opts;

		private IConnection _polygonConnection;
		private IAsyncSubscription _stockQuoteSubscription;
		private IAsyncSubscription _stockAggregatedSubscription;
		private IAsyncSubscription _stockLastTradeSubscription;
		public List<StockLastTrade> StockLastTrades;

		public PolygonService(IOptions<AppSettings> appSettings, IPolygonDataSaver polygonDataSaver)
		{
			_appSettings = appSettings;
			_polygonDataSaver = polygonDataSaver;
			SetupOptions();
			StockLastTrades = new List<StockLastTrade>(_appSettings.Value.DataProcessing.IntraDayBulkCount);
		}

		public void SubscribeToQuotes()
		{
			EnsureConnectionExists();
			_stockQuoteSubscription = _polygonConnection.SubscribeAsync("Q.*", StockQuoteHandlerAsync);
		}

		public void UnsubscribeFromQuotes()
		{
			if (!_stockQuoteSubscription.Connection.IsClosed())
			{
				_stockQuoteSubscription.Unsubscribe();
			}
		}

		public void SubscribeToAggregated()
		{
			EnsureConnectionExists();
			_stockAggregatedSubscription = _polygonConnection.SubscribeAsync("A.*", StockAggregatedHandlerAsync);
		}

		public void UnsubscribeFromAggregated()
		{
			if (!_stockAggregatedSubscription.Connection.IsClosed())
			{
				_stockAggregatedSubscription.Unsubscribe();
			}
		}

		public void SubscribeToTrades()
		{
			Log.Warning("SubscribeToTrades");

			EnsureConnectionExists();
			_stockLastTradeSubscription = _polygonConnection.SubscribeAsync("T.*", StockTradeHandlerAsync);
		}


		public void UnsubscribeFromTrades()
		{
			if (!_stockLastTradeSubscription.Connection.IsClosed())
			{
				_stockLastTradeSubscription.Unsubscribe();
			}
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

			if (MarketHoursHelper.Now() >= MarketHoursHelper.MarketOpenDateTime() && MarketHoursHelper.Now() < MarketHoursHelper.MarketCloseDateTime())
			{
				StockLastTrades.Add(lastTrade);

				if (StockLastTrades.Count == _appSettings.Value.DataProcessing.IntraDayBulkCount)
				{
					_polygonDataSaver.SaveBulkLastTradeData(StockLastTrades);
					Console.WriteLine($"{DateTime.Now:dddd, dd MMMM yyyy HH:mm:ss} - bulk records insterted { StockLastTrades.Count } Trades | Total Size: { NumberExtensions.HumanReadable(StockLastTrades.Sum(x => x.Size), 4) }");
					StockLastTrades = new List<StockLastTrade>(_appSettings.Value.DataProcessing.IntraDayBulkCount);
				}
			}
			else if (MarketHoursHelper.Now() >= MarketHoursHelper.MarketOpenDateTime() && MarketHoursHelper.Now() <= MarketHoursHelper.MarketCloseDateTime().AddMinutes(10))
			{
				StockLastTrades.Add(lastTrade);

				if (StockLastTrades.Count == _appSettings.Value.DataProcessing.AfterMarketBulkCount)
				{
					_polygonDataSaver.SaveBulkLastTradeData(StockLastTrades);
					Console.WriteLine($"{DateTime.Now:dddd, dd MMMM yyyy HH:mm:ss} - bulk records insterted { StockLastTrades.Count } Trades | Total Size: { NumberExtensions.HumanReadable(StockLastTrades.Sum(x => x.Size), 4) }");
					StockLastTrades = new List<StockLastTrade>(_appSettings.Value.DataProcessing.AfterMarketBulkCount);
				}
			}
			else
			{
				//insert into db whatever trades are left
				if (StockLastTrades.Count > 0)
				{
					_polygonDataSaver.SaveBulkLastTradeData(StockLastTrades);
					Console.WriteLine($"{DateTime.Now:dddd, dd MMMM yyyy HH:mm:ss} - bulk records insterted { StockLastTrades.Count } Trades | Total Size: { NumberExtensions.HumanReadable(StockLastTrades.Sum(x => x.Size), 4) }");
					StockLastTrades = new List<StockLastTrade>(_appSettings.Value.DataProcessing.IntraDayBulkCount);
				}
				_polygonDataSaver.SaveLastTradeData(lastTrade);
				Console.WriteLine($"{DateTime.Now:dddd, dd MMMM yyyy HH:mm:ss} -- { lastTrade.Price } | { lastTrade.Ticker } | { lastTrade.Size }");
			}
		}

		private async void StockAggregatedHandlerAsync(object sender, MsgHandlerEventArgs e)
		{
			if (TryGetMessageAsString(e, out var message))
			{
				return;
			}

			var stockAggregated = Deserialize<StockAggregatedModel>(message, new PolygonStockAggregatedConverter());
			if (stockAggregated != null)
			{
				await _polygonDataSaver.SaveAggregatedDataAsync(stockAggregated);
			}
		}

		private async void StockQuoteHandlerAsync(object sender, MsgHandlerEventArgs e)
		{
			if (TryGetMessageAsString(e, out var message))
			{
				return;
			}

			var stockQuote = Deserialize<StockQuoteModel>(message, new PolygonStockQuoteConverter());
			if (stockQuote != null)
			{
				await _polygonDataSaver.SaveQuoteDataAsync(stockQuote);

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
			_stockQuoteSubscription?.Dispose();
			_stockAggregatedSubscription?.Dispose();
			_stockLastTradeSubscription?.Dispose();
		}
	}
}