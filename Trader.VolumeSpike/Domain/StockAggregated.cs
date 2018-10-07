using System;

namespace Trader.VolumeSpike.Domain
{
    public class StockAggregated:Entity
    {
        public string Ticker { get; set; }
        public decimal Open { get; set; }
        public decimal Close { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public DateTime DateTime { get; set; }
        public long Volume { get; set; }
        public override string CollectionName { get; } = "StockAggregated";
    }
}