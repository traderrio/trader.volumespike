using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Trader.Domain;
using Trader.VolumeSpike.Common.Configuration;
using Trader.VolumeSpike.Infrastructure.DbContext.Interfaces;

namespace Trader.VolumeSpike.Infrastructure.DbContext
{
	public class TraderDbContext : BaseDbContext, ITraderDbContext
	{
		private readonly IOptions<AppSettings> _appSettings;

		public TraderDbContext(IMongoDatabase database, IOptions<AppSettings> appSettings)
			: base(database, appSettings)
		{
			_appSettings = appSettings;
			BsonClassMap.RegisterClassMap<ApiKeysContext>();
			BsonClassMap.RegisterClassMap<TokenContext>();
			BsonClassMap.RegisterClassMap<OAuthTokenContext>();
		}
	}
}