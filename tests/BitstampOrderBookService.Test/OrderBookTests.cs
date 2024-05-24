using BitstampOrderBookService.Domain.Entities;

public class OrderBookTests
{
    [Fact]
    public void AddAsk_ShouldAddOrderToAsks()
    {
        // Arrange
        var orderBook = new OrderBook("btcusd", DateTime.UtcNow);
        var order = new Order(50000, 1, "btcusd");

        // Act
        orderBook.AddAsk(order);

        // Assert
        Assert.Contains(order, orderBook.GetAsks());
    }

    [Fact]
    public void AddBid_ShouldAddOrderToBids()
    {
        // Arrange
        var orderBook = new OrderBook("btcusd", DateTime.UtcNow);
        var order = new Order(49000, 1, "btcusd");

        // Act
        orderBook.AddBid(order);

        // Assert
        Assert.Contains(order, orderBook.GetBids());
    }
}
