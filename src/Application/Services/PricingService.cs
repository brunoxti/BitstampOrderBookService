using BitstampOrderBookService.src.Application.Interfaces;
using BitstampOrderBookService.src.Domain.Entities;
using BitstampOrderBookService.src.Domain.ValueObjects;
using BitstampOrderBookService.src.Infrastructure.Data;
using MongoDB.Driver;
using System.Diagnostics.Metrics;

namespace BitstampOrderBookService.src.Application.Services
{
    public class PricingService : IPricingService
    {
        private readonly IMongoCollection<OrderBook> _orderBookCollection;

        public PricingService(IMongoDatabase database)
        {
            _orderBookCollection = database.GetCollection<OrderBook>("orderbooks");
        }
    }
}
