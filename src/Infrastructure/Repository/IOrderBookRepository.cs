using BitstampOrderBookService.src.Domain.Entities;

namespace BitstampOrderBookService.src.Infrastructure.Repository
{
    public interface IOrderBookRepository
    {
        Task<OrderBook> GetLatestOrderBookAsync(string pair);
        Task InsertOrderBookAsync(OrderBook orderBook);
    }
}
