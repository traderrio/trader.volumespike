namespace Trader.VolumeSpike.Common.Configuration
{
	public class AppSettings
	{
		public PolygonSettings Polygon { get; set; }
		public DataProcessingSettings DataProcessing { get; set; }
		public LastTradesSettings LastTrades { get; set; }
		public MicroservicesSettings Microservices{ get; set; }

	}
}
