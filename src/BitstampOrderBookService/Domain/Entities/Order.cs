﻿using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace BitstampOrderBookService.Domain.Entities
{
    public class Order
    {
        [BsonId]
        [JsonIgnore]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; private set; }
        public decimal Price { get; private set; }
        public decimal Quantity { get; private set; }
        public string Pair { get; private set; }

        public Order()
        {
        }

        public Order(decimal price, decimal quantity, string pair)
        {
            Price = price;
            Quantity = quantity;
            Pair = pair;
        }

        public override string ToString()
        {
            return $"{Quantity} {Pair} @ {Price} USD";
        }
    }
}
