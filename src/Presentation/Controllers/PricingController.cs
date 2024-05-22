using BitstampOrderBookService.src.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BitstampOrderBookService.src.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PricingController : ControllerBase
    {
        private readonly PricingService _pricingService;

        public PricingController(PricingService pricingService)
        {
            _pricingService = pricingService;
        }

        [HttpGet("simulate")]
        public async Task<IActionResult> SimulateBestPrice(string operation, string instrument, int quantity)
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
    }
}
