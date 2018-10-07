namespace Trader.VolumeSpike.Common.Configuration
{
    public class DataProcessingSettings
    {
        public int MinimumPrice { get; set; }
	    public int MaximumPrice { get; set; }
	    public int PreMarketBulkCount { get; set; }
	    public int IntraDayBulkCount { get; set; }
	    public int AfterMarketBulkCount { get; set; }
	}
}