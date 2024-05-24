using BitstampOrderBookService.Infrastructure.WebSocket;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BitstampOrderBookService.Tests.UnitTests
{
    public class WebSocketClientTests
    {
        private readonly Mock<IClientWebSocket> _mockClientWebSocket;
        private readonly Mock<ILogger<WebSocketClient>> _mockLogger;
        private readonly WebSocketClient _webSocketClient;

        public WebSocketClientTests()
        {
            _mockClientWebSocket = new Mock<IClientWebSocket>();
            _mockLogger = new Mock<ILogger<WebSocketClient>>();
            _webSocketClient = new WebSocketClient(_mockClientWebSocket.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ConnectAsync_Should_ConnectWebSocket()
        {
            // Arrange
            var uri = new Uri("wss://ws.bitstamp.net");
            var cancellationToken = new CancellationToken();

            _mockClientWebSocket.Setup(x => x.ConnectAsync(uri, cancellationToken))
                                .Returns(Task.CompletedTask)
                                .Verifiable();

            // Act
            await _webSocketClient.ConnectAsync(uri, cancellationToken);

            // Assert
            _mockClientWebSocket.Verify(x => x.ConnectAsync(uri, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task ConnectAsync_Should_RecreateWebSocketIfClosed()
        {
            // Arrange
            var uri = new Uri("wss://ws.bitstamp.net");
            var cancellationToken = new CancellationToken();

            _mockClientWebSocket.Setup(x => x.State).Returns(WebSocketState.Closed);
            _mockClientWebSocket.Setup(x => x.ConnectAsync(uri, cancellationToken))
                                .Returns(Task.CompletedTask)
                                .Verifiable();

            // Act
            await _webSocketClient.ConnectAsync(uri, cancellationToken);

            // Assert
            _mockClientWebSocket.Verify(x => x.ConnectAsync(uri, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task ReceiveMessages_Should_HandleReceivedMessages()
        {
            // Arrange
            var buffer = new byte[1024];
            var segment = new ArraySegment<byte>(buffer);
            var cancellationToken = new CancellationToken();

            var message = "test message";
            var messageBytes = Encoding.UTF8.GetBytes(message);

            var mockWebSocketReceiveResult = new WebSocketReceiveResult(messageBytes.Length, WebSocketMessageType.Text, true);
            _mockClientWebSocket.SetupSequence(x => x.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), cancellationToken))
                .ReturnsAsync(mockWebSocketReceiveResult)
                .ReturnsAsync(new WebSocketReceiveResult(0, WebSocketMessageType.Close, true));

            var receivedMessage = string.Empty;
            _webSocketClient.OnMessageReceived += (msg) =>
            {
                receivedMessage = msg;
            };

            // Act
            var receiveTask = Task.Run(() => _webSocketClient.ReceiveMessages(cancellationToken));

            // Simulate receiving the message
            await Task.Delay(50); // Allow some time for the receive loop to start
            await _webSocketClient.ReceiveAsync(new ArraySegment<byte>(messageBytes), cancellationToken);

            // Allow some time for the message to be processed
            await Task.Delay(300);

            // Assert
            Assert.Equal(message, receivedMessage);

            // Clean up
            cancellationToken.ThrowIfCancellationRequested();
        }

        [Fact]
        public async Task CloseAsync_Should_CloseWebSocket()
        {
            // Arrange
            var closeStatus = WebSocketCloseStatus.NormalClosure;
            var statusDescription = "Normal Closure";
            var cancellationToken = new CancellationToken();

            _mockClientWebSocket.Setup(x => x.CloseAsync(closeStatus, statusDescription, cancellationToken))
                                .Returns(Task.CompletedTask)
                                .Verifiable();

            // Act
            await _webSocketClient.CloseAsync(closeStatus, statusDescription, cancellationToken);

            // Assert
            _mockClientWebSocket.Verify(x => x.CloseAsync(closeStatus, statusDescription, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task SendAsync_Should_SendMessage()
        {
            // Arrange
            var message = "test message";
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(messageBytes);
            var cancellationToken = new CancellationToken();

            _mockClientWebSocket.Setup(x => x.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken))
                                .Returns(Task.CompletedTask)
                                .Verifiable();

            // Act
            await _webSocketClient.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);

            // Assert
            _mockClientWebSocket.Verify(x => x.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task SubscribeOrderBook_Should_SendSubscriptionMessage()
        {
            // Arrange
            var pair = "btcusd";
            var cancellationToken = new CancellationToken();
            var subscribeMessage = new
            {
                @event = "bts:subscribe",
                data = new
                {
                    channel = $"order_book_{pair}"
                }
            };
            var messageJson = JsonSerializer.Serialize(subscribeMessage);
            var bytes = Encoding.UTF8.GetBytes(messageJson);
            var segment = new ArraySegment<byte>(bytes);

            _mockClientWebSocket.Setup(x => x.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken))
                                .Returns(Task.CompletedTask)
                                .Verifiable();

            // Act
            await _webSocketClient.SubscribeOrderBook(pair, cancellationToken);

            // Assert
            _mockClientWebSocket.Verify(x => x.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken), Times.Once);
        }
    }
}
