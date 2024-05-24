using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace BitstampOrderBookService.Infrastructure.WebSocket
{
    public class WebSocketClient : IWebSocketClient
    {
        private ClientWebSocket _clientWebSocket;
        private readonly ILogger<WebSocketClient> _logger;
        private readonly object _syncRoot = new();

        public WebSocketState State => _clientWebSocket.State;

        public WebSocketClient(ILogger<WebSocketClient> logger)
        {
            _clientWebSocket = new ClientWebSocket();
            _logger = logger;
        }

        public event Action<string> OnMessageReceived;

        protected virtual void RaiseMessageReceived(string message)
        {
            OnMessageReceived?.Invoke(message);
        }

        public async Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            try 
            {
                lock (_syncRoot)
                {
                    if (_clientWebSocket == null || _clientWebSocket.State != WebSocketState.Open)
                    {
                        _clientWebSocket?.Dispose();
                        _clientWebSocket = new ClientWebSocket();
                    }
                }

                _logger.LogInformation("Connecting to WebSocket...");
                await _clientWebSocket.ConnectAsync(uri, cancellationToken);
                _logger.LogInformation("WebSocket connection established.");
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public async Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            await _clientWebSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);
        }

        public void Dispose()
        {
            _clientWebSocket.Dispose();
        }

        public async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            return await _clientWebSocket.ReceiveAsync(buffer, cancellationToken);
        }

        public async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            await _clientWebSocket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);
        }

        public async Task SubscribeOrderBook(string pair, CancellationToken cancellationToken)
        {
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

            _logger.LogInformation($"Subscribing to channel: order_book_{pair}");
            await SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);

        }

        public async Task ReceiveMessages(CancellationToken cancellationToken)
        {
            var buffer = new byte[1024 * 16];
            var stringBuilder = new StringBuilder();

            while (State == WebSocketState.Open)
            {
                var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
                }
                else
                {
                    var messagePart = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    stringBuilder.Append(messagePart);

                    if (result.EndOfMessage)
                    {
                        var completeMessage = stringBuilder.ToString();
                        RaiseMessageReceived(completeMessage);
                        stringBuilder.Clear();
                    }
                }
            }
        }
    }
}
