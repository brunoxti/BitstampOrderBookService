using BitstampOrderBookService.Infrastructure.WebSocket;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace BitstampOrderBookService.Tests.UnitTests.Infraestruture
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
