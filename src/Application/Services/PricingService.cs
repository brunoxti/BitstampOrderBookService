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
        private readonly IMongoCollection<PriceSimulationResult> _simulationResultsCollection;

        public PricingService(IMongoDatabase database, IMongoCollection<PriceSimulationResult> simulationResultsCollection)
        {
            _orderBookCollection = database.GetCollection<OrderBook>("orderbooks");
            _simulationResultsCollection = simulationResultsCollection;
        }

        public async Task<PriceSimulationResult> SimulatePriceAsync(string pair, string operation, decimal quantity)
        {
            var filter = Builders<OrderBook>.Filter.Eq(o => o.Pair, pair.ToLower());
            var orderBook = await _orderBookCollection.Find(filter).SortByDescending(o => o.Timestamp).FirstOrDefaultAsync().ConfigureAwait(false) ?? throw new Exception("Order book not found for the given instrument.");
            var orders = operation.ToLower() == "buy"
                ? orderBook.GetAsks().OrderBy(o => o.Price).ToList()
                : orderBook.GetBids().OrderByDescending(o => o.Price).ToList();

            var totalCost = 0m;
            var remainingQuantity = quantity;
            var ordersUsed = new List<Order>();

            foreach (var order in orders)
            {
                if (remainingQuantity <= 0)
                    break;

                var usedQuantity = Math.Min(order.Quantity, remainingQuantity);
                totalCost += usedQuantity * order.Price;
                remainingQuantity -= usedQuantity;

                ordersUsed.Add(new Order(order.Price, usedQuantity, order.Pair));
            }

            if (remainingQuantity > 0)
            {
                throw new Exception("Insufficient quantity available to fulfill the request.");
            }

            var result = new PriceSimulationResult
            {
                Id = Guid.NewGuid(),
                Pair = pair,
                Operation = operation,
                Quantity = quantity,
                TotalCost = totalCost,
                OrdersUsed = ordersUsed
            };

            await _simulationResultsCollection.InsertOneAsync(result).ConfigureAwait(false);

            return result;
        }

    }
}
