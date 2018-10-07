using System;
using Newtonsoft.Json;
using Trader.VolumeSpike.Models;

namespace Trader.VolumeSpike.Infrastructure.JsonConverters
{
    public class PolygonStockQuoteConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanConvert(Type objectType) =>  typeof(StockQuoteModel).IsAssignableFrom(objectType);


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var currentPrices = existingValue == null
                ? new StockQuoteModel()
                : (StockQuoteModel) existingValue;

            var currentProperty = string.Empty;
            reader.FloatParseHandling = FloatParseHandling.Decimal;

            while (reader.Read())
            {
                if (reader.Value != null)
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.PropertyName:
                            currentProperty = reader.Value.ToString();
                            break;
                        case JsonToken.String when currentProperty == "sym":
                            currentPrices.Ticker = reader.Value.ToString();
                            break;
                        case JsonToken.Integer when currentProperty == "bs":
                            currentPrices.BidSize = (long) reader.Value;
                            break;
                        case JsonToken.Float when currentProperty == "bp":
                            currentPrices.Bid = (decimal) reader.Value;
                            break;
                        case JsonToken.Integer when currentProperty == "t":
                            currentPrices.DateTime = epoch.AddMilliseconds((long) reader.Value);
                            break;
                        case JsonToken.Integer when currentProperty == "as":
                            currentPrices.AskSize = (long) reader.Value;
                            break;
                        case JsonToken.Float when currentProperty == "ap":
                            currentPrices.Ask = (decimal) reader.Value;
                            break;
                       
                    }
                }
            }

            return currentPrices;
        }
    }
}