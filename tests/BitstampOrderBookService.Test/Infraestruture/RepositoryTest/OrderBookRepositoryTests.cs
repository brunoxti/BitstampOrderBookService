using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BitstampOrderBookService.Domain.Entities;
using BitstampOrderBookService.Infrastructure.Repository;
using MongoDB.Driver;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace BitstampOrderBookService.Tests.UnitTests.Infraestruture.RepositoryTest
{
    public class OrderBookRepositoryTests
    {
        private readonly Mock<IMongoCollection<OrderBook>> _mockOrderBookCollection;
        private readonly Mock<IMongoDatabase> _mockDatabase;
        private readonly Mock<ILogger<OrderBookRepository>> _mockLogger;
        private readonly IOrderBookRepository _orderBookRepository;

        public OrderBookRepositoryTests()
        {
            _mockOrderBookCollection = new Mock<IMongoCollection<OrderBook>>();
            _mockDatabase = new Mock<IMongoDatabase>();
            _mockLogger = new Mock<ILogger<OrderBookRepository>>();
            _mockDatabase.Setup(db => db.GetCollection<OrderBook>("orderbooks", null)).Returns(_mockOrderBookCollection.Object);

            _orderBookRepository = new OrderBookRepository(_mockDatabase.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task InsertOrderBookAsync_Should_InsertOrderBook()
        {
            // Arrange
            var orderBook = new OrderBook("btcusd", DateTime.UtcNow);

            // Act
            await _orderBookRepository.InsertOrderBookAsync(orderBook);

            // Assert
            _mockOrderBookCollection.Verify(col => col.InsertOneAsync(orderBook, null, default), Times.Once);
        }

        [Fact]
        public async Task InsertOrderBookAsync_Should_LogError_WhenExceptionThrown()
        {
            // Arrange
            var orderBook = new OrderBook("btcusd", DateTime.UtcNow);
            _mockOrderBookCollection.Setup(col => col.InsertOneAsync(orderBook, null, default))
                                    .ThrowsAsync(new Exception("Insertion error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _orderBookRepository.InsertOrderBookAsync(orderBook));
            Assert.Equal("Insertion error", exception.Message);

            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error inserting OrderBook")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public async Task FindOrderBooksAsync_Should_ReturnOrderBooks()
        {
            // Arrange
            var filter = Builders<OrderBook>.Filter.Eq(ob => ob.Pair, "btcusd");
            var findOptions = new FindOptions<OrderBook>();
            var mockCursor = new Mock<IAsyncCursor<OrderBook>>();
            var orderBooks = new List<OrderBook> { new OrderBook("btcusd", DateTime.UtcNow) };

            mockCursor.Setup(_ => _.Current).Returns(orderBooks);
            mockCursor
                .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Returns(Task.FromResult(false));

            _mockOrderBookCollection.Setup(col => col.FindAsync(It.IsAny<FilterDefinition<OrderBook>>(), It.IsAny<FindOptions<OrderBook>>(), It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _orderBookRepository.FindOrderBooksAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("btcusd", result[0].Pair);
        }

        [Fact]
        public async Task FindOrderBooksAsync_Should_LogError_WhenExceptionThrown()
        {
            // Arrange
            var filter = Builders<OrderBook>.Filter.Eq(ob => ob.Pair, "btcusd");

            // Configurar o mock do cursor
            var mockCursor = new Mock<IAsyncCursor<OrderBook>>();
            mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Returns(Task.FromResult(false));

            _mockOrderBookCollection.Setup(col => col.FindAsync(filter, It.IsAny<FindOptions<OrderBook>>(), default))
                                    .ReturnsAsync(mockCursor.Object);

            // Configurar a exceção no método FindAsync para ser lançada após a configuração do mock do cursor
            _mockOrderBookCollection.Setup(col => col.FindAsync(filter, It.IsAny<FindOptions<OrderBook>>(), default))
                                    .ThrowsAsync(new Exception("Find error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _orderBookRepository.FindOrderBooksAsync(filter));
            Assert.Equal("Find error", exception.Message);

            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error finding OrderBooks")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}
