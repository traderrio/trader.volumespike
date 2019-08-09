using System;
using System.Collections.Generic;
using System.Globalization;
using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Trader.Common.MessagePackFormatters;
using Trader.Common.Product;

namespace Trader.VolumeSpike.Infrastructure
{
    [MessagePackObject]
    public class ClientOptionsWs
    {
        public ClientOptionsWs()
        {
            Products = new List<StockProductModel>();
            MessageTypes = new List<StreamingMessageType>();
        }
        
        [Key("sendPricesBack")]
        public bool SendPricesBack { get; set; }

        [Key("products")]
        public IList<StockProductModel> Products { get; set; }

        [Key("allProducts")]
        public bool AllProducts { get; set; }

        [Key("messageTypes")]
        public IList<StreamingMessageType> MessageTypes { get; set; }
    }

    [MessagePackObject]
    public abstract class StreamingMessage
    {
        [Key("mt")]
        public abstract StreamingMessageType MessageType { get; }

        [IgnoreMember]
        public abstract DataServerType ServerType { get; }

        [JsonProperty("sym")]
        [Key("sym")]
        public string Ticker { get; set; }

        [JsonConverter(typeof(EpochInMillisecondsJsonConverter))]
        [JsonProperty("t")]
        [Key("t")]
        public virtual DateTime DateTime { get; set; }
    }

    public class StreamMessageConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var messages = new List<StreamingMessage>();
            var jObject = JToken.ReadFrom(reader);
            if (jObject is JArray)
            {
                foreach (var jToken in jObject)
                {
                    var message = ParseMessage(serializer, jToken);
                    if (message != null)
                    {
                        messages.Add(message);
                    }
                }
            }
            else
            {
                throw new Exception("Need to support single object also");
            }

            return messages;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(StreamingMessage) ||
                   objectType == typeof(IList<StreamingMessage>);
        }

        private StreamingMessage ParseMessage(JsonSerializer serializer, JToken jToken)
        {
            var typeString = (StreamingMessageType) jToken["messageType"].Value<int>();

            StreamingMessage result;
            switch (typeString)
            {
                case StreamingMessageType.Status:
                    result = new StreamingStatusMessage();
                    break;
                case StreamingMessageType.StockLastTrade:
                    result = new StockLastTradeMessage();
                    break;
                case StreamingMessageType.StockLastQuote:
                    result = new StockLastQuoteMessage();
                    break;
                case StreamingMessageType.StockSecondAggregated:
                case StreamingMessageType.StockMinuteAggregated:
                    result = new StockSecondAggregatedMessage();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            serializer.Populate(jToken.CreateReader(), result);

            return result;
        }
    }

    public enum StreamingMessageType
    {
        None,
        Status,
        StockLastTrade,
        StockLastQuote,
        StockSecondAggregated,
        StockMinuteAggregated
    }

    public class StreamingStatusMessage : StreamingMessage
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        public override StreamingMessageType MessageType => StreamingMessageType.Status;
        public override DataServerType ServerType => DataServerType.Stocks;
    }

    public enum DataServerType
    {
        None,
        Stocks,
        Forex,
        Crypto
    }

    public class StockSecondAggregatedMessage : BaseStockAggregatedMessage
    {
        public override StreamingMessageType MessageType => StreamingMessageType.StockSecondAggregated;
        public override DataServerType ServerType => DataServerType.Stocks;
    }

    [MessagePackObject]
    public abstract class BaseStockAggregatedMessage : StreamingMessage
    {
        [JsonProperty("op")]
        [Key("o")]
        [MessagePackFormatter(typeof(DecimalFormatter))]
        public decimal Open { get; set; }

        [JsonProperty("c")]
        [Key("c")]
        [MessagePackFormatter(typeof(DecimalFormatter))]
        public decimal Close { get; set; }

        [JsonProperty("h")]
        [Key("h")]
        [MessagePackFormatter(typeof(DecimalFormatter))]
        public decimal High { get; set; }

        [JsonProperty("l")]
        [Key("l")]
        [MessagePackFormatter(typeof(DecimalFormatter))]
        public decimal Low { get; set; }

        [JsonProperty("a")]
        [IgnoreMember]
        public decimal Average { get; set; }

        [JsonProperty("s")]
        [JsonConverter(typeof(EpochInMillisecondsJsonConverter))]
        [Key("t")]
        public override DateTime DateTime { get; set; }

        [JsonProperty("e")]
        [JsonConverter(typeof(EpochInMillisecondsJsonConverter))]
        [IgnoreMember]
        public DateTime End { get; set; }

        [JsonProperty("v")]
        [Key("v")]
        public long Volume { get; set; }

        [JsonProperty("av")]
        [IgnoreMember]
        public long AccumulatedVolume { get; set; }

        [JsonProperty("vw")]
        [IgnoreMember]
        public long Vwap { get; set; }
    }

    [MessagePackObject]
    public class StockLastTradeMessage : StreamingMessage
    {
        public override StreamingMessageType MessageType => StreamingMessageType.StockLastTrade;
        public override DataServerType ServerType => DataServerType.Stocks;

        [JsonProperty("x")]
        [IgnoreMember]
        public int Exchange { get; set; }

        [JsonProperty("p")]
        [Key("p")]
        [MessagePackFormatter(typeof(DecimalFormatter))]
        public decimal Price { get; set; }

        [JsonProperty("s")]
        [IgnoreMember]
        public long Size { get; set; }
    }

    [MessagePackObject]
    public class StockLastQuoteMessage : StreamingMessage
    {
        public override StreamingMessageType MessageType => StreamingMessageType.StockLastQuote;
        public override DataServerType ServerType => DataServerType.Stocks;

        [JsonProperty("ap")]
        [Key("ap")]
        [MessagePackFormatter(typeof(DecimalFormatter))]
        public decimal Ask { get; set; }

        [JsonProperty("bp")]
        [Key("bp")]
        [MessagePackFormatter(typeof(DecimalFormatter))]
        public decimal Bid { get; set; }

        [JsonProperty("as")]
        [Key("as")]
        public long AskSize { get; set; }

        [JsonProperty("bs")]
        [Key("bs")]
        public long BidSize { get; set; }
    }

    public class EpochInMillisecondsJsonConverter : DateTimeConverterBase
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue(((DateTime) value - Epoch).TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                return null;
            }

            return Epoch.AddMilliseconds((long) reader.Value);
        }
    }
}