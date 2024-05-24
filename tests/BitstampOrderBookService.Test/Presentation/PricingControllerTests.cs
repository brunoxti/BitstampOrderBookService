using BitstampOrderBookService.Application.Interfaces;
using BitstampOrderBookService.Domain.Entities;
using BitstampOrderBookService.Domain.ValueObjects;
using BitstampOrderBookService.Presentation.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

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
        public async Task GetAllSimulations_Should_ReturnOkResult_WithAllSimulations()
        {
            // Arrange
            var simulations = new List<PriceSimulationResult>
            {
                new PriceSimulationResult { Pair = "btcusd" },
                new PriceSimulationResult { Pair = "ethusd" }
            };

            _mockPricingService.Setup(service => service.GetAllSimulationsAsync())
                .ReturnsAsync(simulations);

            // Act
            var result = await _controller.GetAllSimulations();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<PriceSimulationResult>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task SimulateBestPrice_Should_ReturnOkResult_WithSimulationResult()
        {
            // Arrange
            var simulationResult = new PriceSimulationResult { Pair = "btcusd" };

            _mockPricingService.Setup(service => service.SimulatePriceAsync("btcusd", "buy", 1))
                .ReturnsAsync(simulationResult);

            // Act
            var result = await _controller.SimulateBestPrice("buy", "btcusd", 1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PriceSimulationResult>(okResult.Value);
            Assert.Equal("btcusd", returnValue.Pair);
        }

        [Fact]
        public async Task SimulateBestPrice_Should_ReturnBadRequest_OnException()
        {
            // Arrange
            _mockPricingService.Setup(service => service.SimulatePriceAsync("btcusd", "buy", 1))
                .ThrowsAsync(new System.Exception("Simulation error"));

            // Act
            var result = await _controller.SimulateBestPrice("buy", "btcusd", 1);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Simulation error", badRequestResult.Value);
        }
    }
}
