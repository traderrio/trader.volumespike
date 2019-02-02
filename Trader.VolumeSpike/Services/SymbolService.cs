using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Trader.Common.Enums;
using Trader.Domain;
using Trader.VolumeSpike.Infrastructure.DbContext.Interfaces;
using Trader.VolumeSpike.Services.Interfaces;

namespace Trader.VolumeSpike.Services
{
	public class SymbolService : ISymbolService
	{
		private readonly IMongoCollection<SymbolDetails> _symbolCollection;
		public SymbolService(ITraderDbContext dbContext)
		{
			_symbolCollection = dbContext.GetCollection<SymbolDetails>();
		}

		public List<SymbolDetails> GetValidSymbols()
		{
			return _symbolCollection
									.AsQueryable()
									.Where(x => x.TypeEnum == SymbolType.Stock && !x.IsOtc)
									.ToList();
		}
	}
}