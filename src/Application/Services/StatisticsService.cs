using BitstampOrderBookService.src.Application.Interfaces;
using BitstampOrderBookService.src.Domain.Entities;
using BitstampOrderBookService.src.Domain.ValueObjects;
using BitstampOrderBookService.src.Infrastructure.Data;
using MongoDB.Driver;

namespace BitstampOrderBookService.src.Application.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IMongoCollection<OrderBook> _orderBookCollection;

        public StatisticsService(IMongoDatabase database)
        {
            _orderBookCollection = database.GetCollection<OrderBook>("orderbooks");
        }
    }
}
