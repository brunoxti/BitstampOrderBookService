using BitstampOrderBookService.Application.Services;
using BitstampOrderBookService.Domain.Entities;
using BitstampOrderBookService.Domain.ValueObjects;
using MongoDB.Driver;
using Moq;


namespace BitstampOrderBookService.Tests.UnitTests
{
    public class StatisticsServiceTests
    {
        private readonly Mock<IMongoCollection<OrderBook>> _mockOrderBookCollection;
        private readonly StatisticsService _statisticsService;

        public StatisticsServiceTests()
        {
            _mockOrderBookCollection = new Mock<IMongoCollection<OrderBook>>();

            var mockDatabase = new Mock<IMongoDatabase>();
            mockDatabase.Setup(db => db.GetCollection<OrderBook>("orderbooks", null))
                .Returns(_mockOrderBookCollection.Object);

            _statisticsService = new StatisticsService(mockDatabase.Object);
        }

        [Fact]
        public void PrintStatistics_ShouldPrintCorrectStatistics()
        {
            // Arrange
            var orderBook = new OrderBook("btcusd", DateTime.UtcNow);
            orderBook.AddAsk(new Order(50000, 1, "btcusd"));
            orderBook.AddAsk(new Order(51000, 1, "btcusd"));
            orderBook.AddBid(new Order(49000, 1, "btcusd"));
            orderBook.AddBid(new Order(48000, 1, "btcusd"));

            var orderBooks = new List<OrderBook> { orderBook };
            var mockCursor = CreateAsyncCursor(orderBooks);

            _mockOrderBookCollection.Setup(c => c.FindSync(It.IsAny<FilterDefinition<OrderBook>>(), It.IsAny<FindOptions<OrderBook>>(), It.IsAny<CancellationToken>()))
                .Returns(mockCursor);

            // Act & Assert
            var statisticsTask = Task.Run(() => _statisticsService.PrintStatistics());
            statisticsTask.Wait(5000);//aguardndo a saida de 5 segundos
        }

        private IAsyncCursor<OrderBook> CreateAsyncCursor(IEnumerable<OrderBook> documents)
        {
            var mockCursor = new Mock<IAsyncCursor<OrderBook>>();
            mockCursor.SetupSequence(_ => _.Current).Returns(documents);
            mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);

            return mockCursor.Object;
        }
    }
}
