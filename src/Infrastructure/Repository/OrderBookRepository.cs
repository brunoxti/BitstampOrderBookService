using BitstampOrderBookService.src.Domain.Entities;
using BitstampOrderBookService.src.Infrastructure.Data;
using MongoDB.Driver;

namespace BitstampOrderBookService.src.Infrastructure.Repository
{
    public class OrderBookRepository : IOrderBookRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderBookRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task InsertOrderBookAsync(OrderBook orderBook)
        {
            await _context.OrderBooks.ReplaceOneAsync(
                ob => ob.Pair == orderBook.Pair && ob.Timestamp == orderBook.Timestamp,
                orderBook,
                new ReplaceOptions { IsUpsert = true }
            );
        }

        public async Task<OrderBook> GetLatestOrderBookAsync(string pair)
        {
            return await _context.OrderBooks
                .Find(ob => ob.Pair == pair)
                .SortByDescending(ob => ob.Timestamp)
                .FirstOrDefaultAsync();
        }
    }
}
