using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Flurl.Http;
using MessagePack;
using Microsoft.Extensions.Logging;
using Trader.Dto;
using Trader.VolumeSpike.Common.Configuration;
using Trader.VolumeSpike.Infrastructure;
using Trader.VolumeSpike.Services.Interfaces;

namespace Trader.VolumeSpike.Services
{
	public class SymbolService : ISymbolService
	{
        private readonly IOptions<AppSettings> _appSettings;
        private readonly ILogger<ISymbolService> _logger;

		public SymbolService(IOptions<AppSettings> appSettings, ILogger<ISymbolService> logger)
		{
            _appSettings = appSettings;
            _logger = logger;
        }

		public List<SymbolDetailsDto> GetValidSymbols()
		{
            var url = $"{_appSettings.Value.Microservices.TraderrApi}/api/integration/symbol-details";

            var response =  url.GetJsonAsync<BinaryIntegrationResponse>().GetAwaiter().GetResult();

            if (!response.Success)
            {
                _logger.LogError(response.Message);
                return new List<SymbolDetailsDto>();
            }

            var symbols = MessagePackSerializer.Deserialize<IList<SymbolDetailsDto>>(response.Result, MessagePack.Resolvers.ContractlessStandardResolver.Instance);

			return symbols
                        .Where(x => !x.IsOtc)
                        .ToList();
		}
	}
}