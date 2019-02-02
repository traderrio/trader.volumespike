using System;
using Newtonsoft.Json;
using Trader.Domain;

namespace Trader.VolumeSpike.Infrastructure.JsonConverters
{
	public class PolygonStockTradeConverter : JsonConverter
	{
		public override bool CanWrite => false;
		public override bool CanConvert(Type objectType) => typeof(StockLastTrade).IsAssignableFrom(objectType);


		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			var stockLastTrade = existingValue == null
				? new StockLastTrade()
				: (StockLastTrade)existingValue;

			var currentProperty = string.Empty;
			reader.FloatParseHandling = FloatParseHandling.Decimal;

			try
			{
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
								stockLastTrade.Ticker = reader.Value.ToString();
								break;
							case JsonToken.Integer when currentProperty == "x":
								stockLastTrade.Exchange = (long)reader.Value;
								break;
							case JsonToken.Float when currentProperty == "p":
								stockLastTrade.Price = (decimal)reader.Value;
								break;
							case JsonToken.Integer when currentProperty == "s":
								stockLastTrade.Size = (long)reader.Value;
								break;
							case JsonToken.Integer when currentProperty == "t":
								stockLastTrade.DateTime = epoch.AddMilliseconds((long)reader.Value);
								break;
						}
					}
				}
			}
			catch (Exception e)
			{
			}

			return stockLastTrade;
		}
	}
}