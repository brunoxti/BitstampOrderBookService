using BitstampOrderBookService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BitstampOrderBookService.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PricingController : ControllerBase
    {
        private readonly IPricingService _pricingService;

        public PricingController(IPricingService pricingService)
        {
            _pricingService = pricingService;
        }

        [HttpGet("simulate")]
        public async Task<IActionResult> SimulateBestPrice(string operation, string instrument, decimal quantity)
        {
            try
            {
                var result = await _pricingService.SimulatePriceAsync(instrument, operation, quantity);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("simulations")]
        public async Task<IActionResult> GetAllSimulations()
        {
            var results = await _pricingService.GetAllSimulationsAsync();
            return Ok(results);
        }
    }
}
