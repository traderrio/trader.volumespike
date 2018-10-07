using System;

namespace Trader.VolumeSpike.Models
{
    public class StockQuoteModel
    {
        public string Ticker { get; set; }
        public decimal Ask { get; set; }
        public decimal Bid { get; set; }
        public DateTime DateTime { get; set; }
        public long AskSize { get; set; }
        public long BidSize { get; set; }
    }
}