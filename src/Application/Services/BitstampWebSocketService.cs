using BitstampOrderBookService.src.Application.DTOs;
using BitstampOrderBookService.src.Application.Interfaces;
using BitstampOrderBookService.src.Domain.Entities;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace BitstampOrderBookService.src.Application.Services
{
    public class BitstampWebSocketService : IBitstampWebSocketService
    {
        private ClientWebSocket _webSocket;
        private readonly IMongoCollection<OrderBook> _orderBookCollection;
        private readonly ConcurrentDictionary<string, OrderBook> _orderBooks = new();

        public BitstampWebSocketService(IMongoDatabase database)
        {
            _orderBookCollection = database.GetCollection<OrderBook>("orderbooks");
        }

        public async Task ConnectAsync(string[] pairs)
        {
            _webSocket = new ClientWebSocket();

            await _webSocket.ConnectAsync(new Uri("wss://ws.bitstamp.net"), CancellationToken.None);

            foreach (var pair in pairs)
            {
                await SubscribeToChannelAsync(pair);
            }
        }

        private async Task SubscribeToChannelAsync(string pair)
        {
            var subscribeMessage = new
            {
                @event = "bts:subscribe",
                data = new
                {
                    channel = $"order_book_{pair}"
                }
            };

            var messageJson = JsonSerializer.Serialize(subscribeMessage);
            var bytes = Encoding.UTF8.GetBytes(messageJson);
            var segment = new ArraySegment<byte>(bytes);

            await _webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);

            await ReceiveMessageAsync();
        }

        private async Task ReceiveMessageAsync()
        {
            var buffer = new byte[1024 * 16];
            var stringBuilder = new StringBuilder();

            while (_webSocket.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    var messagePart = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    stringBuilder.Append(messagePart);

                    if (result.EndOfMessage)
                    {
                        var completeMessage = stringBuilder.ToString();
                        await HandleWebSocketMessageAsync(completeMessage);
                        stringBuilder.Clear();
                    }
                }
            }
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
