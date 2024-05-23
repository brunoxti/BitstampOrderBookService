using BitstampOrderBookService.Domain.Entities;
using BitstampOrderBookService.Infrastructure.Data;
using MongoDB.Driver;

namespace BitstampOrderBookService.Infrastructure.Repository
{
    public class OrderBookRepository : IOrderBookRepository
    {
        private readonly IMongoCollection<OrderBook> _orderBookCollection;
        private readonly ILogger<OrderBookRepository> _logger;

        public OrderBookRepository(IMongoDatabase database, ILogger<OrderBookRepository> logger)
        {
            _orderBookCollection = database.GetCollection<OrderBook>("orderbooks");
            _logger = logger;
        }

        public async Task InsertOrderBookAsync(OrderBook orderBook)
        {
            try
            {
                await _orderBookCollection.InsertOneAsync(orderBook);
                _logger.LogInformation($"Inserted OrderBook for {orderBook.Pair} at {orderBook.Timestamp}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inserting OrderBook: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<List<OrderBook>> FindOrderBooksAsync(FilterDefinition<OrderBook> filter, FindOptions options = null)
        {
            try
            {
                return await _orderBookCollection.Find(filter, options).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error finding OrderBooks: {ex.Message}", ex);
                throw;
            }
        }
    }
}
