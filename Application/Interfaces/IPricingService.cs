using BitstampOrderBookService.Domain.ValueObjects;

namespace BitstampOrderBookService.Application.Interfaces
{
    public interface IPricingService
    {
        Task<PriceSimulationResult> SimulatePriceAsync(string pair, string operation, decimal quantity);
    }
}
