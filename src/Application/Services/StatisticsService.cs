using BitstampOrderBookService.src.Application.Interfaces;
using BitstampOrderBookService.src.Domain.Entities;
using BitstampOrderBookService.src.Domain.ValueObjects;
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

        public void PrintStatistics()
        {
            while (true)
            {
                Thread.Sleep(5000);

                var btcUsd = GetStattistics("btcusd");
                var ethUsd = GetStattistics("ethusd");

                Console.WriteLine($"BTC/USD - Maior: {btcUsd.Highest}, Menor: {btcUsd.Lowest}, Média Preço: {btcUsd.Average}, Média Preço Acumulada: {btcUsd.AccumulatedAverage}, Média Quantidade Acumulada: {btcUsd.AccumulatedQuantityAverage}");
                Console.WriteLine($"ETH/USD - Maior: {ethUsd.Highest}, Menor: {ethUsd.Lowest}, Média Preço: {ethUsd.Average}, Média Preço Acumulada: {ethUsd.AccumulatedAverage}, Média Quantidade Acumulada: {ethUsd.AccumulatedQuantityAverage}");

            }
        }

        private Statistics GetStattistics(string pair)
        {
            var filter = Builders<OrderBook>.Filter.Eq(o => o.Pair, pair);
            var orderBooks = _orderBookCollection.Find(filter).ToList();

            var allAsks = orderBooks.SelectMany(o => o.Asks).ToList();
            var allBids = orderBooks.SelectMany(o => o.Bids).ToList();

            // Calculate highest ask and lowest bid
            var highestAsk = allAsks.Any() ? allAsks.Max(a => a.Price) : (decimal?)null;
            var lowestBid = allBids.Any() ? allBids.Min(b => b.Price) : (decimal?)null;

            // Calculate average price
            var totalOrders = allAsks.Count + allBids.Count;
            var averagePrice = totalOrders > 0 ? (allAsks.Sum(a => a.Price) + allBids.Sum(b => b.Price)) / totalOrders : (decimal?)null;

            // Calculate accumulated average price over the last 5 seconds
            var fiveSecondsAgo = DateTime.UtcNow.AddSeconds(-5);
            var recentOrderBooks = orderBooks.Where(ob => ob.Timestamp >= fiveSecondsAgo).ToList();
            var recentAsks = recentOrderBooks.SelectMany(o => o.Asks).ToList();
            var recentBids = recentOrderBooks.SelectMany(o => o.Bids).ToList();
            var recentTotalOrders = recentAsks.Count + recentBids.Count;
            var accumulatedAverage = recentTotalOrders > 0 ? (recentAsks.Sum(a => a.Price) + recentBids.Sum(b => b.Price)) / recentTotalOrders : (decimal?)null;

            // Calculate accumulated average quantity of each active
            var totalQuantity = allAsks.Sum(a => a.Quantity) + allBids.Sum(b => b.Quantity);
            var accumulatedQuantityAverage = totalOrders > 0 ? totalQuantity / totalOrders : (decimal?)null;

            return new Statistics
            {
                Highest = highestAsk,
                Lowest = lowestBid,
                Average = averagePrice,
                AccumulatedAverage = accumulatedAverage,
                AccumulatedQuantityAverage = accumulatedQuantityAverage
            };
        }
    }
}
