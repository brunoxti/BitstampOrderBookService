using System;
using System.Collections.Generic;
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

namespace BitstampOrderBookService.Tests.UnitTests
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
            var pairs = new[] { "btcusd", "ethusd" };
            var uri = "wss://ws.bitstamp.net";
            
            var cancellationToken = CancellationToken.None;

            // Act
            await _service.ConnectAsync(uri, cancellationToken);

            // Assert
            foreach (var pair in pairs)
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

    }
}
