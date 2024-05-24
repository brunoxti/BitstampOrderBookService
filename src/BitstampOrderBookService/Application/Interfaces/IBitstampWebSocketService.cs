namespace BitstampOrderBookService.Application.Interfaces
{
    public interface IBitstampWebSocketService
    {
        Task ConnectAsync(string Uri, CancellationToken cancellationToken);
    }
}
