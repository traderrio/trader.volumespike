using System;
using Newtonsoft.Json;
using Trader.VolumeSpike.Models;

namespace Trader.VolumeSpike.Infrastructure.JsonConverters
{
    public class PolygonStockAggregatedConverter:JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanConvert(Type objectType) => typeof(StockAggregatedModel).IsAssignableFrom(objectType);


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var currentPrices = existingValue == null
                ? new StockAggregatedModel()
                : (StockAggregatedModel) existingValue;

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

                        case JsonToken.Integer when currentProperty == "v":
                            currentPrices.Volume = (long) reader.Value;
                            break;
                        case JsonToken.Float when currentProperty == "o":
                            currentPrices.Open = (decimal) reader.Value;
                            break;
                        case JsonToken.Float when currentProperty == "c":
                            currentPrices.Close = (decimal) reader.Value;
                            break;
                        case JsonToken.Float when currentProperty == "l":
                            currentPrices.Low = (decimal) reader.Value;
                            break;
                        case JsonToken.Integer when currentProperty == "e":
                            currentPrices.DateTime = epoch.AddMilliseconds((long) reader.Value);
                            break;
                        case JsonToken.Float when currentProperty == "h":
                            currentPrices.High = (decimal) reader.Value;
                            break;
                    }
                }
            }

            return currentPrices;
        }
    }
}