using BitstampOrderBookService.Presentation.Controllers;
using BitstampOrderBookService.Application.Interfaces;
using BitstampOrderBookService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System;
using System.Threading.Tasks;

namespace BitstampOrderBookService.Tests.UnitTests
{
    public class PricingControllerTests
    {
        private readonly Mock<IPricingService> _mockPricingService;
        private readonly PricingController _controller;

        public PricingControllerTests()
        {
            _mockPricingService = new Mock<IPricingService>();
            _controller = new PricingController(_mockPricingService.Object);
        }

        [Fact]
        public async Task SimulateBestPrice_Should_ReturnOkResult_WithPriceSimulationResult()
        {
            // Arrange
            var operation = "buy";
            var instrument = "btcusd";
            var quantity = 1.0m;
            var result = new PriceSimulationResult { TotalCost = 1000 };

            _mockPricingService.Setup(service => service.SimulatePriceAsync(instrument, operation, quantity))
                               .ReturnsAsync(result);

            // Act
            var response = await _controller.SimulateBestPrice(operation, instrument, quantity);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(result, okResult.Value);
        }

        [Fact]
        public async Task SimulateBestPrice_Should_ReturnBadRequest_WhenExceptionThrown()
        {
            // Arrange
            var operation = "buy";
            var instrument = "btcusd";
            var quantity = 1.0m;

            _mockPricingService.Setup(service => service.SimulatePriceAsync(instrument, operation, quantity))
                               .ThrowsAsync(new Exception("Simulation error"));

            // Act
            var response = await _controller.SimulateBestPrice(operation, instrument, quantity);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal("Simulation error", badRequestResult.Value);
        }
    }
}
