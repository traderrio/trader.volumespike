using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl.Http;
using MessagePack;
using Microsoft.Extensions.Options;
using Trader.Domain;
using Trader.VolumeSpike.Common.Configuration;
using Trader.VolumeSpike.Infrastructure;
using Trader.VolumeSpike.Services.Interfaces;

namespace Trader.VolumeSpike.Services
{
	public class LastTradesService : ILastTradesService
	{
        private readonly IOptions<AppSettings> _appSettings;

		public LastTradesService(IOptions<AppSettings> appSettings)
		{
            _appSettings = appSettings;
        }

        public async Task<List<StockLastTrade>> GetLastTrades(string ticker, DateTime fromDate, DateTime to, int length, int timeFrame)
		{
            var url = $"{_appSettings.Value.Microservices.PolygonApi}/api/trading-activity/last-trades?ticker={ticker}&fromDate={fromDate}&to={to}&length={length}&timeFrame={timeFrame}";

            var response = await url.GetJsonAsync<BinaryIntegrationResponse>();

            if (!response.Success)
            {
                return new List<StockLastTrade>();
            }

            var data = MessagePackSerializer.Deserialize<List<StockLastTrade>>(response.Result, MessagePack.Resolvers.ContractlessStandardResolver.Instance);

            return data;
        }
	}
}