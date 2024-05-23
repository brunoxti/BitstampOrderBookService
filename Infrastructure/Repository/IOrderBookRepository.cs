using BitstampOrderBookService.Domain.Entities;

namespace BitstampOrderBookService.Infrastructure.Repository
{
    public interface IOrderBookRepository
    {
        Task<OrderBook> GetLatestOrderBookAsync(string pair);
        Task InsertOrderBookAsync(OrderBook orderBook);
    }
}
