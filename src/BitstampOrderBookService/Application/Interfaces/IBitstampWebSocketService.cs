namespace BitstampOrderBookService.Application.Interfaces
{
    public interface IBitstampWebSocketService
    {
        Task HandleWebSocketMessageAsync(string message);
        Task ConnectAsync(string[] intruments);
    }
}
