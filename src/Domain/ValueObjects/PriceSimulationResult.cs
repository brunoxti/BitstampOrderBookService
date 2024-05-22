using BitstampOrderBookService.src.Domain.Entities;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BitstampOrderBookService.src.Domain.ValueObjects
{

    public class PriceSimulationResult
    {
        [BsonId]
        public Guid Id { get; set; }
        public string Pair { get; set; }
        public string Operation { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalCost { get; set; }
        public List<Order> OrdersUsed { get; set; }
    }
}
