using System.Collections.Generic;
using Trader.Dto;

namespace Trader.VolumeSpike.Services.Interfaces
{
	public interface ISymbolService
	{
		List<SymbolDetailsDto> GetValidSymbols();
	}
}