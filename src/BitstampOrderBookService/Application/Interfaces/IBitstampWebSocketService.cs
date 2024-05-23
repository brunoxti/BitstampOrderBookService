namespace BitstampOrderBookService.Application.Interfaces
{
    public interface IBitstampWebSocketService
    {
        Task ConnectAsync(string[] intruments, CancellationToken cancellationToken);
    }
}
