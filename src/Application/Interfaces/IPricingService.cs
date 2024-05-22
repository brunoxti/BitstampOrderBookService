using BitstampOrderBookService.src.Domain.ValueObjects;

namespace BitstampOrderBookService.src.Application.Interfaces
{
    public interface IPricingService
    {
        Task<PriceSimulationResult> SimulatePriceAsync(string pair, string operation, decimal quantity);
    }
}
