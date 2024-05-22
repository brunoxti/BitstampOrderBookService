using BitstampOrderBookService.src.Domain.Entities;
using MongoDB.Driver;

namespace BitstampOrderBookService.src.Infrastructure.Data
{
    public class ApplicationDbContext
    {
        private readonly IMongoDatabase _database;

        public ApplicationDbContext(IMongoDatabase database)
        {
            _database = database;
        }

        public IMongoCollection<OrderBook> OrderBooks => _database.GetCollection<OrderBook>("orderbooks");
    }
}
