using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Trader.VolumeSpike.Common.Configuration;
using Trader.VolumeSpike.Infrastructure.DbContext.Interfaces;

namespace Trader.VolumeSpike.Infrastructure.DbContext
{
	public class TraderDbContext : BaseDbContext, ITraderDbContext
	{
		public TraderDbContext(IMongoDatabase database, IOptions<AppSettings> appSettings)
			: base(database, appSettings)
		{
		}
	}
}