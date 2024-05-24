using MongoDB.Driver;
using Moq;
using BitstampOrderBookService.Application.Services;
using BitstampOrderBookService.Domain.Entities;
using BitstampOrderBookService.Domain.ValueObjects;
using BitstampOrderBookService.Infrastructure.Repository;

namespace BitstampOrderBookService.Tests.UnitTests
{
    public class PricingServiceTests
    {
        private readonly Mock<IOrderBookRepository> _mockOrderBookRepository;
        private readonly Mock<IMongoCollection<PriceSimulationResult>> _mockSimulationResultsCollection;
        private readonly PricingService _pricingService;

        public PricingServiceTests()
        {
            _mockOrderBookRepository = new Mock<IOrderBookRepository>();
            _mockSimulationResultsCollection = new Mock<IMongoCollection<PriceSimulationResult>>();

            _pricingService = new PricingService(_mockOrderBookRepository.Object, _mockSimulationResultsCollection.Object);
        }

        [Fact]
        public async Task SimulatePriceAsync_BuyOperation_Success()
        {
            // Arrange
            var orderBook = new OrderBook("btcusd", DateTime.UtcNow);
            orderBook.AddAsk(new Order(50000, 1, "btcusd"));
            orderBook.AddAsk(new Order(51000, 1, "btcusd"));

            var orderBooks = new List<OrderBook> { orderBook };

            _mockOrderBookRepository.Setup(r => r.FindOrderBooksAsync(It.IsAny<FilterDefinition<OrderBook>>(), null))
                .ReturnsAsync(orderBooks);

            // Act
            var result = await _pricingService.SimulatePriceAsync("btcusd", "buy", 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(50000, result.TotalCost);
            Assert.Single(result.OrdersUsed);
        }

        [Fact]
        public async Task SimulatePriceAsync_SellOperation_Success()
        {
            // Arrange
            var orderBook = new OrderBook("btcusd", DateTime.UtcNow);
            orderBook.AddBid(new Order(49000, 1, "btcusd"));
            orderBook.AddBid(new Order(48000, 1, "btcusd"));

            var orderBooks = new List<OrderBook> { orderBook };

            _mockOrderBookRepository.Setup(r => r.FindOrderBooksAsync(It.IsAny<FilterDefinition<OrderBook>>(), null))
                .ReturnsAsync(orderBooks);

            // Act
            var result = await _pricingService.SimulatePriceAsync("btcusd", "sell", 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(49000, result.TotalCost);
            Assert.Single(result.OrdersUsed);
        }

        [Fact]
        public async Task SimulatePriceAsync_InsufficientQuantity_ThrowsException()
        {
            // Arrange
            var orderBook = new OrderBook("btcusd", DateTime.UtcNow);
            orderBook.AddAsk(new Order(50000, 0.5m, "btcusd"));

            var orderBooks = new List<OrderBook> { orderBook };

            _mockOrderBookRepository.Setup(r => r.FindOrderBooksAsync(It.IsAny<FilterDefinition<OrderBook>>(), null))
                .ReturnsAsync(orderBooks);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _pricingService.SimulatePriceAsync("btcusd", "buy", 1));
        }

        [Fact]
        public async Task GetAllSimulationsAsync_Should_ReturnAllSimulations()
        {
            // Arrange
            var mockAsyncCursor = new Mock<IAsyncCursor<PriceSimulationResult>>();
            var simulationResults = new List<PriceSimulationResult>
            {
                new PriceSimulationResult { Pair = "btcusd" },
                new PriceSimulationResult { Pair = "ethusd" }
            };

            mockAsyncCursor.Setup(_ => _.Current).Returns(simulationResults);
            mockAsyncCursor
                .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);
            mockAsyncCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Returns(Task.FromResult(false));

            _mockSimulationResultsCollection
                .Setup(c => c.FindAsync(It.IsAny<FilterDefinition<PriceSimulationResult>>(), It.IsAny<FindOptions<PriceSimulationResult>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockAsyncCursor.Object);

            // Act
            var results = await _pricingService.GetAllSimulationsAsync();

            // Assert
            Assert.Equal(2, results.Count);
            Assert.Contains(results, r => r.Pair == "btcusd");
            Assert.Contains(results, r => r.Pair == "ethusd");
        }
    }
}
