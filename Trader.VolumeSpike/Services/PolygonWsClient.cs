using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Easy.MessageHub;
using MessagePack;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Trader.Common.Enums;
using Trader.Common.Product;
using Trader.VolumeSpike.Common.Configuration;
using Trader.VolumeSpike.Infrastructure;

namespace Trader.VolumeSpike.Services
{
    public interface IPolygonWsClient
    {
        void RegisterProducts(IList<IProductModel> pendingProducts);
        void RegisterAll();
    }

    [MessagePackObject]
    public class RegisterStockProductWs
    {
        [Key("product")]
        public StockProductModel Product { get; set; }
    }

    public class PolygonWsClient : IPolygonWsClient
    {
        private readonly ILogger<IPolygonWsClient> _logger;
        private readonly HubConnection _connection;
        private IList<IProductModel> _products;
        private bool _allProducts;

        public PolygonWsClient(IMessageHub messageHub, IOptions<AppSettings> appSettings,
            ILogger<IPolygonWsClient> logger)
        {
            _logger = logger;
            _products = new List<IProductModel>();

            var url = $"{appSettings.Value.Microservices.PolygonApi}/stocks/prices";

            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogCritical("Polygon websocket url is null");
                return;
            }

            _connection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();

            _connection.On<JArray>("AssetsPricesUpdated", (data) =>
            {
                var str = data.ToString();
                var res = JsonConvert.DeserializeObject<IList<StreamingMessage>>(str, new StreamMessageConverter());

                messageHub.Publish(res);
            });

            _connection.Closed += ConnectionOnClosed;

            Connect();
        }

        private async void Connect()
        {
            try
            {
                _logger.LogInformation("Connecting to polygon WS...");
                await _connection.StartAsync();
                _logger.LogInformation("Connection to polygon WS is established.");

                if (_allProducts)
                {
                    RegisterAll();
                }
                else if (_products.Count > 0)
                {
                    RegisterProducts(_products);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to connect to polygon WS.", e);

                await ReconnectAsync();
            }
        }

        private async Task ReconnectAsync()
        {
            _logger.LogError("Reconnecting to polygon WS...");
            await Task.Delay(TimeSpan.FromSeconds(3));
            Connect();
        }


        private async Task ConnectionOnClosed(Exception arg)
        {
            await ReconnectAsync();
        }

        public void RegisterProducts(IList<IProductModel> products)
        {
            _products = products;
            _allProducts = false;
            if (_connection.State != HubConnectionState.Connected)
            {
                return;
            }

            var productsName = string.Join(", ", products.Select(p => p.Ticker));
            _logger.LogInformation($"Registering products: {productsName}");


            var stockProducts = products.Where(p => p.Type == SymbolType.Stock).ToList();

          
            var handleOptions = new ClientOptionsWs
            {
                Products = stockProducts.Cast<StockProductModel>().ToList(),
                MessageTypes = new List<StreamingMessageType>
                {
                    StreamingMessageType.StockLastTrade
                }
            };
            
            _connection.SendAsync("HandleStockProductsMessage", handleOptions);
        }

        public void RegisterAll()
        {
            _allProducts = true;
            _products = new List<IProductModel>();

            if (_connection.State != HubConnectionState.Connected)
            {
                return;
            }

            var handleOptions = new ClientOptionsWs
            {
                AllProducts = true,
                MessageTypes = new List<StreamingMessageType>
                {
                    StreamingMessageType.StockLastTrade
                }
            };

            try
            {
                _connection.SendAsync("HandleStockProductsMessage", handleOptions).Wait();

            }
            catch (Exception e)
            {
                _logger.LogError(e, "HandleStockProductsMessage");
            }
        }
    }
}