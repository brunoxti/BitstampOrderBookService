using BitstampOrderBookService.Domain.Entities;
using MongoDB.Driver;

namespace BitstampOrderBookService.Infrastructure.Repository
{
    public interface IOrderBookRepository
    {
        Task InsertOrderBookAsync(OrderBook orderBook);
        Task<List<OrderBook>> FindOrderBooksAsync(FilterDefinition<OrderBook> filter, FindOptions options = null);
    }
}
