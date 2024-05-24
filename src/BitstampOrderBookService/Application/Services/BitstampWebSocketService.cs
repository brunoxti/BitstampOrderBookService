using BitstampOrderBookService.Application.DTOs;
using BitstampOrderBookService.Application.Interfaces;
using BitstampOrderBookService.Configuration;
using BitstampOrderBookService.Domain.Entities;
using BitstampOrderBookService.Infrastructure.Repository;
using BitstampOrderBookService.Infrastructure.WebSocket;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Text.Json;

namespace BitstampOrderBookService.Application.Services
{
    public class BitstampWebSocketService : IBitstampWebSocketService
    {
        private readonly IOrderBookRepository _orderBookRepository;
        private IWebSocketClient _webSocketClient;
        private readonly ConcurrentDictionary<string, OrderBook> _orderBooks = new();
        private readonly ValidPairsOptions _validPairs;

        public BitstampWebSocketService(IWebSocketClient webSocketClient, IOrderBookRepository orderBookRepository, IOptions<ValidPairsOptions> validPairsOptions)
        {
            _orderBookRepository = orderBookRepository;
            _validPairs = validPairsOptions?.Value ?? throw new ArgumentNullException(nameof(validPairsOptions));
            _webSocketClient = webSocketClient;
            _webSocketClient.OnMessageReceived += async (message) => await HandleWebSocketMessageAsync(message);
        }
        
        public async Task ConnectAsync(string uri, CancellationToken cancellationToken)
        {
            await _webSocketClient.ConnectAsync(new Uri(uri), cancellationToken);

            foreach (var pair in _validPairs.Pairs)
            {
                if (_validPairs.Pairs.Contains(pair.ToLower()))
                {
                    await _webSocketClient.SubscribeOrderBook(pair, cancellationToken);
                }
                else
                {
                    Console.WriteLine($"Pair {pair} is not valid.");
                }
            }

            Task.Run(() => _webSocketClient.ReceiveMessages(cancellationToken), cancellationToken);
        }

        public async Task HandleWebSocketMessageAsync(string message)
        {
            var orderBookUpdate = JsonSerializer.Deserialize<OrderBookUpdate>(message);
            if (orderBookUpdate != null && orderBookUpdate.Event == "data")
            {
                var pair = orderBookUpdate.Channel.Replace("order_book_", "").ToLower();
                
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
               
                await _orderBookRepository.InsertOrderBookAsync(orderBook);
            }
        }
    }
}
