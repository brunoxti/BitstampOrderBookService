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
    }
}
