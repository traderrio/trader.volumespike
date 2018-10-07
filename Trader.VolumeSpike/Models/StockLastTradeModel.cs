using System;

namespace Trader.VolumeSpike.Models
{
    public class StockLastTradeModel
    {
        public string Ticker { get; set; }
        public decimal Price { get; set; }
        public long Size { get; set; }
        public DateTime DateTime { get; set; }
        public long Exchange { get; set; }
    }
}