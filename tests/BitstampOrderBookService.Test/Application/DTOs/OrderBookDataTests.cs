using BitstampOrderBookService.Application.DTOs;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace BitstampOrderBookService.Tests.UnitTests
{
    public class OrderBookDataTests
    {
        [Fact]
        public void OrderBookData_Should_DeserializeFromJson()
        {
            // Arrange
            var json = @"
            {
                ""timestamp"": ""1629364856"",
                ""microtimestamp"": ""1629364856123456"",
                ""bids"": [
                    [""123.45"", ""67.89""],
                    [""124.45"", ""68.89""]
                ],
                ""asks"": [
                    [""125.45"", ""69.89""],
                    [""126.45"", ""70.89""]
                ]
            }";

            // Act
            var orderBookData = JsonSerializer.Deserialize<OrderBookData>(json);

            // Assert
            Assert.NotNull(orderBookData);
            Assert.Equal("1629364856", orderBookData.Timestamp);
            Assert.Equal("1629364856123456", orderBookData.MicroTimestamp);
            Assert.Equal(2, orderBookData.Bids.Count);
            Assert.Equal(2, orderBookData.Asks.Count);
            Assert.Equal("123.45", orderBookData.Bids[0][0]);
            Assert.Equal("67.89", orderBookData.Bids[0][1]);
            Assert.Equal("125.45", orderBookData.Asks[0][0]);
            Assert.Equal("69.89", orderBookData.Asks[0][1]);
        }

        [Fact]
        public void OrderBookData_Should_Have_DefaultValues()
        {
            // Arrange & Act
            var orderBookData = new OrderBookData();

            // Assert
            Assert.Null(orderBookData.Timestamp);
            Assert.Null(orderBookData.MicroTimestamp);
            Assert.Null(orderBookData.Bids);
            Assert.Null(orderBookData.Asks);
        }
    }
}
