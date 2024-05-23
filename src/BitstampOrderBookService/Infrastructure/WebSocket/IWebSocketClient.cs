using BitstampOrderBookService.Application.DTOs;
using System.Net.WebSockets;

namespace BitstampOrderBookService.Infrastructure.WebSocket
{
    public interface IWebSocketClient : IDisposable
    {
        WebSocketState State { get; }
        event Action<string> OnMessageReceived;

        Task ConnectAsync(Uri uri, CancellationToken cancellationToken);
        Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken);
        Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);
        Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken);
        Task SubscribeOrderBook(string pair);
        Task ReceiveMessages(CancellationToken cancellationToken);
    }
}
