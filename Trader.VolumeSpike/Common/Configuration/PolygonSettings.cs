namespace Trader.VolumeSpike.Common.Configuration
{
    public class PolygonSettings
    {
        public string ApiKey { get; set; }
        public string[] StockServers { get; set; }
        public PolygonDataSaverType SaverType { get; set; }
    }
}