using BitstampOrderBookService.Domain.Entities;
using System;
using System.Collections.Generic;
using Xunit;

namespace BitstampOrderBookService.Tests.UnitTests
{
    public class OrderBookTests
    {
        [Fact]
        public void DefaultConstructor_Should_InitializeProperties()
        {
            // Act
            var orderBook = new OrderBook();

            // Assert
            Assert.NotNull(orderBook.Asks);
            Assert.NotNull(orderBook.Bids);
            Assert.Empty(orderBook.Asks);
            Assert.Empty(orderBook.Bids);
            Assert.Null(orderBook.Pair);
            Assert.Equal(default(DateTime), orderBook.Timestamp);
        }

        [Fact]
        public void ParameterizedConstructor_Should_InitializeProperties()
        {
            // Arrange
            var pair = "btcusd";
            var timestamp = DateTime.UtcNow;

            // Act
            var orderBook = new OrderBook(pair, timestamp);

            // Assert
            Assert.NotNull(orderBook.Asks);
            Assert.NotNull(orderBook.Bids);
            Assert.Empty(orderBook.Asks);
            Assert.Empty(orderBook.Bids);
            Assert.Equal(pair, orderBook.Pair);
            Assert.Equal(timestamp, orderBook.Timestamp);
        }

        [Fact]
        public void AddAsk_Should_AddOrderToAsks()
        {
            // Arrange
            var orderBook = new OrderBook();
            var ask = new Order(123.45m, 67.89m, "btcusd");

            // Act
            orderBook.AddAsk(ask);

            // Assert
            Assert.Single(orderBook.Asks);
            Assert.Contains(ask, orderBook.Asks);
        }

        [Fact]
        public void AddBid_Should_AddOrderToBids()
        {
            // Arrange
            var orderBook = new OrderBook();
            var bid = new Order(123.45m, 67.89m, "btcusd");

            // Act
            orderBook.AddBid(bid);

            // Assert
            Assert.Single(orderBook.Bids);
            Assert.Contains(bid, orderBook.Bids);
        }

        [Fact]
        public void GetAsks_Should_ReturnReadOnlyListOfAsks()
        {
            // Arrange
            var orderBook = new OrderBook();
            var ask = new Order(123.45m, 67.89m, "btcusd");
            orderBook.AddAsk(ask);

            // Act
            var asks = orderBook.GetAsks();

            // Assert
            Assert.Single(asks);
            Assert.Contains(ask, asks);
        }

        [Fact]
        public void GetBids_Should_ReturnReadOnlyListOfBids()
        {
            // Arrange
            var orderBook = new OrderBook();
            var bid = new Order(123.45m, 67.89m, "btcusd");
            orderBook.AddBid(bid);

            // Act
            var bids = orderBook.GetBids();

            // Assert
            Assert.Single(bids);
            Assert.Contains(bid, bids);
        }
    }
}
