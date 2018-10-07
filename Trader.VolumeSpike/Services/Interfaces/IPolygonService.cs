namespace Trader.VolumeSpike.Services.Interfaces
{
    public interface IPolygonService
    {
        void SubscribeToQuotes();
        void SubscribeToAggregated();
        void SubscribeToTrades();
        void UnsubscribeFromQuotes();
        void UnsubscribeFromAggregated();
        void UnsubscribeFromTrades();
    }
}