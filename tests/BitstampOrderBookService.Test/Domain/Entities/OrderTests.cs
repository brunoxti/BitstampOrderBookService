using BitstampOrderBookService.Domain.Entities;
using Xunit;

namespace BitstampOrderBookService.Tests.UnitTests
{
    public class OrderTests
    {
        [Fact]
        public void Constructor_Should_InitializeProperties()
        {
            // Arrange
            decimal price = 123.45m;
            decimal quantity = 67.89m;
            string pair = "btcusd";

            // Act
            var order = new Order(price, quantity, pair);

            // Assert
            Assert.Equal(price, order.Price);
            Assert.Equal(quantity, order.Quantity);
            Assert.Equal(pair, order.Pair);
        }

        [Fact]
        public void ToString_Should_ReturnFormattedString()
        {
            // Arrange
            decimal price = 123.45m;
            decimal quantity = 67.89m;
            string pair = "btcusd";
            var order = new Order(price, quantity, pair);
            string expectedString = $"{quantity} {pair} @ {price} USD";

            // Act
            var result = order.ToString();

            // Assert
            Assert.Equal(expectedString, result);
        }

        [Fact]
        public void DefaultConstructor_Should_SetIdToNull()
        {
            // Act
            var order = new Order();

            // Assert
            Assert.Null(order.Id);
        }
    }
}
