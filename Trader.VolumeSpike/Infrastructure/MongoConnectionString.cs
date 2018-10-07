using MongoDB.Driver;

namespace Trader.VolumeSpike.Infrastructure
{
	public class MongoConnectionString
	{
		public MongoClientSettings Settings { get; set; }
		public string Database { get; set; }
		public MongoConnectionString(string connectionString)
		{
			var mongoUrl = MongoUrl.Create(connectionString);
			Database = mongoUrl.DatabaseName;
			Settings = MongoClientSettings.FromUrl(mongoUrl);
		}
	}
}
