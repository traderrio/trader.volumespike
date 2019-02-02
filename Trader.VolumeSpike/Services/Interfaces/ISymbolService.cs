using System.Collections.Generic;
using Trader.Domain;

namespace Trader.VolumeSpike.Services.Interfaces
{
	public interface ISymbolService
	{
		List<SymbolDetails> GetValidSymbols();
	}
}