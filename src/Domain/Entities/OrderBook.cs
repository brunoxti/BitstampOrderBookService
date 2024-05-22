using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BitstampOrderBookService.src.Domain.Entities
{
    public class OrderBook
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; private set; }
        public string Pair { get; private set; }
        public DateTime Timestamp { get; private set; }
        public List<Order> Asks { get; private set; }
        public List<Order> Bids { get; private set; }

        public OrderBook()
        {
            Asks = new List<Order>();
            Bids = new List<Order>();
        }

        public OrderBook(string pair, DateTime timestamp)
        {
            Pair = pair;
            Timestamp = timestamp;
            Asks = new List<Order>();
            Bids = new List<Order>();
        }

        public void AddAsk(Order ask)
        {
            Asks.Add(ask);
        }

        public void AddBid(Order bid)
        {
            Bids.Add(bid);
        }
     
        public IReadOnlyList<Order> GetAsks() => Asks.AsReadOnly();
        public IReadOnlyList<Order> GetBids() => Bids.AsReadOnly();
    }
}
