using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BitstampOrderBookService.Application.DTOs;
using BitstampOrderBookService.Application.Interfaces;
using BitstampOrderBookService.Application.Services;
using BitstampOrderBookService.Configuration;
using BitstampOrderBookService.Domain.Entities;
using BitstampOrderBookService.Infrastructure.Repository;
using BitstampOrderBookService.Infrastructure.WebSocket;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BitstampOrderBookService.Tests.UnitTests.Application.Services
{
    public class BitstampWebSocketServiceTests
    {
        private readonly Mock<IWebSocketClient> _mockWebSocketClient;
        private readonly Mock<IOrderBookRepository> _mockOrderBookRepository;
        private readonly BitstampWebSocketService _service;
        private readonly IOptions<ValidPairsOptions> _validPairsOptions;

        public BitstampWebSocketServiceTests()
        {
            _mockWebSocketClient = new Mock<IWebSocketClient>();
            _mockOrderBookRepository = new Mock<IOrderBookRepository>();

            _validPairsOptions = Options.Create(new ValidPairsOptions
            {
                Pairs = new List<string> { "btcusd", "ethusd" }
            });

            _service = new BitstampWebSocketService(_mockWebSocketClient.Object, _mockOrderBookRepository.Object, _validPairsOptions);
        }

        [Fact]
        public async Task ConnectAsync_Should_SubscribeToOrderBooks()
        {
            // Arrange
            var uri = "wss://ws.bitstamp.net";
            var cancellationToken = CancellationToken.None;

            // Act
            await _service.ConnectAsync(uri, cancellationToken);

            // Assert
            foreach (var pair in _validPairsOptions.Value.Pairs)
            {
                _mockWebSocketClient.Verify(client => client.SubscribeOrderBook(pair, cancellationToken), Times.Once);
            }
            _mockWebSocketClient.Verify(client => client.ReceiveMessages(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task HandleWebSocketMessageAsync_Should_ProcessOrderBookUpdate()
        {
            // Arrange
            var message = "{\"event\":\"data\",\"channel\":\"order_book_btcusd\",\"data\":{\"timestamp\":\"1609459200\",\"bids\":[[\"30000.0\",\"0.5\"]],\"asks\":[[\"31000.0\",\"1.0\"]]}}";

            // Act
            await _service.HandleWebSocketMessageAsync(message);

            // Assert
            _mockOrderBookRepository.Verify(repo => repo.InsertOrderBookAsync(
                It.Is<OrderBook>(ob => ob.Pair == "btcusd" && ob.Asks.Count == 1 && ob.Bids.Count == 1)), Times.Once);
        }

        [Fact]
        public async Task HandleWebSocketMessageAsync_Should_NotProcessInvalidMessage()
        {
            // Arrange
            string invalidMessage = "{ \"event\": \"invalid\" }";

            // Act
            await _service.HandleWebSocketMessageAsync(invalidMessage);

            // Assert
            _mockOrderBookRepository.Verify(repo => repo.InsertOrderBookAsync(It.IsAny<OrderBook>()), Times.Never);
        }
        [Fact]
        public async Task ConnectAsync_Should_NotSubscribeToInvalidPairs()
        {
            // Arrange
            var validPairsOptions = Options.Create(new ValidPairsOptions
            {
                Pairs = new List<string> { "btcusd" }
            });

            var service = new BitstampWebSocketService(_mockWebSocketClient.Object, _mockOrderBookRepository.Object, validPairsOptions);
            var uri = "wss://ws.bitstamp.net";
            var cancellationToken = CancellationToken.None;

            // Act
            await service.ConnectAsync(uri, cancellationToken);

            // Assert
            _mockWebSocketClient.Verify(client => client.SubscribeOrderBook("btcusd", cancellationToken), Times.Once);
            _mockWebSocketClient.Verify(client => client.SubscribeOrderBook("invalidpair", cancellationToken), Times.Never);
        }

        [Fact]
        public async Task HandleWebSocketMessageAsync_Should_NotProcessMessageWithInvalidTimestamp()
        {
            // Arrange
            var invalidTimestampMessage = "{\"event\":\"data\",\"channel\":\"order_book_btcusd\",\"data\":{\"timestamp\":\"invalid\",\"bids\":[[\"30000.0\",\"0.5\"]],\"asks\":[[\"31000.0\",\"1.0\"]]}}";

            // Act & Assert
            await Assert.ThrowsAsync<FormatException>(() => _service.HandleWebSocketMessageAsync(invalidTimestampMessage));
        }

        [Fact]
        public async Task HandleWebSocketMessageAsync_Should_NotProcessMessageWithInvalidPrice()
        {
            // Arrange
            var invalidPriceMessage = "{\"event\":\"data\",\"channel\":\"order_book_btcusd\",\"data\":{\"timestamp\":\"1609459200\",\"bids\":[[\"invalid\",\"0.5\"]],\"asks\":[[\"31000.0\",\"1.0\"]]}}";

            // Act & Assert
            await Assert.ThrowsAsync<FormatException>(() => _service.HandleWebSocketMessageAsync(invalidPriceMessage));
        }

        [Fact]
        public async Task HandleWebSocketMessageAsync_Should_NotProcessMessageWithInvalidQuantity()
        {
            // Arrange
            var invalidQuantityMessage = "{\"event\":\"data\",\"channel\":\"order_book_btcusd\",\"data\":{\"timestamp\":\"1609459200\",\"bids\":[[\"30000.0\",\"invalid\"]],\"asks\":[[\"31000.0\",\"1.0\"]]}}";

            // Act & Assert
            await Assert.ThrowsAsync<FormatException>(() => _service.HandleWebSocketMessageAsync(invalidQuantityMessage));
        }

        [Fact]
        public async Task HandleWebSocketMessageAsync_Should_IgnoreUnknownChannel()
        {
            // Arrange
            var message = "{\"event\":\"data\",\"channel\":\"unknown_channel\",\"data\":{\"timestamp\":\"1609459200\",\"bids\":[[\"30000.0\",\"0.5\"]],\"asks\":[[\"31000.0\",\"1.0\"]]}}";

            // Act
            await _service.HandleWebSocketMessageAsync(message);

            // Assert
            _mockOrderBookRepository.Verify(repo => repo.InsertOrderBookAsync(It.IsAny<OrderBook>()), Times.Never);
        }

        [Fact]
        public async Task ConnectAsync_Should_HandleValidAndInvalidPairs()
        {
            // Arrange
            var validPairsOptions = Options.Create(new ValidPairsOptions
            {
                Pairs = new List<string> { "btcusd", "ethusd" }
            });

            var service = new BitstampWebSocketService(_mockWebSocketClient.Object, _mockOrderBookRepository.Object, validPairsOptions);
            var uri = "wss://ws.bitstamp.net";
            var cancellationToken = CancellationToken.None;

            // Act
            await service.ConnectAsync(uri, cancellationToken);

            // Assert
            _mockWebSocketClient.Verify(client => client.SubscribeOrderBook("btcusd", cancellationToken), Times.Once);
            _mockWebSocketClient.Verify(client => client.SubscribeOrderBook("ethusd", cancellationToken), Times.Once);
            _mockWebSocketClient.Verify(client => client.SubscribeOrderBook("invalidpair", cancellationToken), Times.Never);
        }

        [Fact]
        public async Task HandleWebSocketMessageAsync_Should_NotProcessMessageWithNullEvent()
        {
            // Arrange
            var message = "{\"channel\":\"order_book_btcusd\",\"data\":{\"timestamp\":\"1609459200\",\"bids\":[[\"30000.0\",\"0.5\"]],\"asks\":[[\"31000.0\",\"1.0\"]]}}";

            // Act
            await _service.HandleWebSocketMessageAsync(message);

            // Assert
            _mockOrderBookRepository.Verify(repo => repo.InsertOrderBookAsync(It.IsAny<OrderBook>()), Times.Never);
        }

        [Fact]
        public async Task HandleWebSocketMessageAsync_Should_ThrowExceptionForInvalidJson()
        {
            // Arrange
            var invalidJsonMessage = "{\"event\":\"data\",\"channel\":\"order_book_btcusd\",\"data\":\"invalidjson\"}";

            // Act & Assert
            await Assert.ThrowsAsync<JsonException>(() => _service.HandleWebSocketMessageAsync(invalidJsonMessage));
        }

        [Fact]
        public async Task HandleWebSocketMessageAsync_Should_ProcessValidOrderBookUpdate()
        {
            // Arrange
            var message = "{\"event\":\"data\",\"channel\":\"order_book_btcusd\",\"data\":{\"timestamp\":\"1609459200\",\"bids\":[[\"30000.0\",\"0.5\"]],\"asks\":[[\"31000.0\",\"1.0\"]]}}";

            // Act
            await _service.HandleWebSocketMessageAsync(message);

            // Assert
            _mockOrderBookRepository.Verify(repo => repo.InsertOrderBookAsync(It.Is<OrderBook>(ob => ob.Pair == "btcusd" && ob.Asks.Count == 1 && ob.Bids.Count == 1)), Times.Once);
        }

        [Fact]
        public void Constructor_Should_ThrowArgumentNullException_When_ValidPairsOptionsIsNull()
        {
            // Arrange & Act
            Action act = () => new BitstampWebSocketService(_mockWebSocketClient.Object, _mockOrderBookRepository.Object, null);

            // Assert
            Assert.Throws<ArgumentNullException>(act);
        }

        [Fact]
        public async Task ConnectAsync_Should_HandleExceptions()
        {
            // Arrange
            var uri = "wss://ws.bitstamp.net";
            var cancellationToken = CancellationToken.None;
            _mockWebSocketClient.Setup(client => client.SubscribeOrderBook(It.IsAny<string>(), cancellationToken))
                                .ThrowsAsync(new Exception("Subscription error"));

            var consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);

            // Act
            await _service.ConnectAsync(uri, cancellationToken);

            // Assert
            var output = consoleOutput.ToString();
            Assert.Contains("Subscription error", output);
        }

        [Fact]
        public async Task HandleWebSocketMessageAsync_Should_NotProcessIfOrderBookUpdateIsNull()
        {
            // Arrange
            string nullMessage = "{}";

            // Act
            await _service.HandleWebSocketMessageAsync(nullMessage);

            // Assert
            _mockOrderBookRepository.Verify(repo => repo.InsertOrderBookAsync(It.IsAny<OrderBook>()), Times.Never);
        }

    }
}
