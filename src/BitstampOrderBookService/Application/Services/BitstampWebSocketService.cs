using BitstampOrderBookService.Application.DTOs;
using BitstampOrderBookService.Application.Interfaces;
using BitstampOrderBookService.Domain.Entities;
using BitstampOrderBookService.Infrastructure.WebSocket;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Text.Json;

namespace BitstampOrderBookService.Application.Services
{
    public class BitstampWebSocketService : IBitstampWebSocketService
    {
        private readonly IWebSocketClient _webSocketClient;
        private readonly IMongoCollection<OrderBook> _orderBookCollection;
        private readonly ConcurrentDictionary<string, OrderBook> _orderBooks = new();

        public BitstampWebSocketService(IWebSocketClient webSocketClient, IMongoDatabase database)
        {
            _webSocketClient = webSocketClient;
            _orderBookCollection = database.GetCollection<OrderBook>("orderbooks");
        }

        public async Task ConnectAsync(string[] pairs, CancellationToken cancellationToken)
        {
          

            await _webSocketClient.ConnectAsync(new Uri("wss://ws.bitstamp.net"), CancellationToken.None);

            foreach (var pair in pairs)
            {
                await _webSocketClient.SubscribeOrderBook(pair);
            }

            _webSocketClient.OnMessageReceived += async (message) =>
            {
                await HandleWebSocketMessageAsync(message);
            };

            await _webSocketClient.ReceiveMessages(cancellationToken);
        }

        public async Task HandleWebSocketMessageAsync(string message)
        {
            var orderBookUpdate = JsonSerializer.Deserialize<OrderBookUpdate>(message);
            if (orderBookUpdate != null && orderBookUpdate.Event == "data")
            {
                var pair = orderBookUpdate.Channel.Replace("order_book_", "").ToLower(); // Usar minúsculas para consistência
                var orderBook = new OrderBook(
                    pair,
                    DateTimeOffset.FromUnixTimeSeconds(long.Parse(orderBookUpdate.Data.Timestamp)).UtcDateTime
                );

                foreach (var ask in orderBookUpdate.Data.Asks)
                {
                    orderBook.AddAsk(new Order(decimal.Parse(ask[0]), decimal.Parse(ask[1]), pair));
                }

                foreach (var bid in orderBookUpdate.Data.Bids)
                {
                    orderBook.AddBid(new Order(decimal.Parse(bid[0]), decimal.Parse(bid[1]), pair));
                }

                _orderBooks[orderBook.Pair] = orderBook;
                await _orderBookCollection.InsertOneAsync(orderBook);
            }
        }
    }
}
