using System;

namespace Trader.VolumeSpike.Domain
{
    public class StockLastTrade : Entity
    {
        public string Ticker { get; set; }
        public decimal Price { get; set; }
        public long Size { get; set; }
        public DateTime DateTime { get; set; }
        public long Exchange { get; set; }
        
        public override string CollectionName => "StockLastTrades";
    }
}