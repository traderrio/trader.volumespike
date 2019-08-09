namespace Trader.VolumeSpike.Common.Configuration
{
    public class DataProcessingSettings
    {
        public int MinimumPrice { get; set; }
	    public int MaximumPrice { get; set; }
	    public int TimeFrame { get; set; }
	    public int RecordLength { get; set; }
	    public int RatioPercentage { get; set; }
	}
}