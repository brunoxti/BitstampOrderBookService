﻿using BitstampOrderBookService.Application.Interfaces;
using BitstampOrderBookService.Domain.Entities;
using BitstampOrderBookService.Domain.ValueObjects;
using BitstampOrderBookService.Infrastructure.Repository;
using MongoDB.Driver;
using System.Diagnostics.Metrics;

namespace BitstampOrderBookService.Application.Services
{
    public class PricingService : IPricingService
    {
        private readonly IOrderBookRepository _orderBookRepository;
        private readonly IMongoCollection<PriceSimulationResult> _simulationResultsCollection;

        public PricingService(IOrderBookRepository orderBookRepository, IMongoCollection<PriceSimulationResult> simulationResultsCollection)
        {
            _orderBookRepository = orderBookRepository;
            _simulationResultsCollection = simulationResultsCollection;
        }

        public async Task<PriceSimulationResult> SimulatePriceAsync(string pair, string operation, decimal quantity)
        {
            var filter = Builders<OrderBook>.Filter.Eq(o => o.Pair, pair.ToLower());
            var orderBooks = await _orderBookRepository.FindOrderBooksAsync(filter);

            if (!orderBooks.Any())
            {
                throw new Exception("Order book not found for the given instrument.");
            }
            var orderBook = orderBooks.OrderByDescending(o => o.Timestamp).FirstOrDefault();

            var orders = operation.ToLower() == "buy"
                ? orderBook.GetAsks().OrderBy(o => o.Price).ToList()
                : orderBook.GetBids().OrderByDescending(o => o.Price).ToList();

            var totalCost = 0m;
            var remainingQuantity = quantity;
            var ordersUsed = new List<Order>();

            foreach (var order in orders)
            {
                if (remainingQuantity <= 0)
                    break;

                var usedQuantity = Math.Min(order.Quantity, remainingQuantity);
                totalCost += usedQuantity * order.Price;
                remainingQuantity -= usedQuantity;

                ordersUsed.Add(new Order(order.Price, usedQuantity, order.Pair));
            }

            if (remainingQuantity > 0)
            {
                throw new Exception("Insufficient quantity available to fulfill the request.");
            }

            var result = new PriceSimulationResult
            {
                Id = Guid.NewGuid(),
                Pair = pair,
                Operation = operation,
                Quantity = quantity,
                TotalCost = totalCost,
                OrdersUsed = ordersUsed
            };

            await _simulationResultsCollection.InsertOneAsync(result).ConfigureAwait(false);

            return result;
        }

        public async Task<List<PriceSimulationResult>> GetAllSimulationsAsync()
        {
            return await _simulationResultsCollection.Find(_ => true).ToListAsync();
        }

    }
}
