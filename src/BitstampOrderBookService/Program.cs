using BitstampOrderBookService.Application.Interfaces;
using BitstampOrderBookService.Application.Services;
using BitstampOrderBookService.Configuration;
using BitstampOrderBookService.Domain.ValueObjects;
using BitstampOrderBookService.Infrastructure.Data;
using BitstampOrderBookService.Infrastructure.Repository;
using BitstampOrderBookService.Infrastructure.WebSocket;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection(nameof(MongoDbSettings)));

builder.Services.Configure<ValidPairsOptions>(
    builder.Configuration.GetSection("ValidPairs"));

builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var client = new MongoClient(settings.ConnectionString);
    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.AddSingleton<ApplicationDbContext>();

builder.Services.AddSingleton<IWebSocketClient, WebSocketClient>();
builder.Services.AddScoped<IOrderBookRepository, OrderBookRepository>();
builder.Services.AddScoped<IBitstampWebSocketService, BitstampWebSocketService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IPricingService, PricingService>();

builder.Services.AddSingleton(sp =>
{
    var database = sp.GetRequiredService<IMongoDatabase>();
    return database.GetCollection<PriceSimulationResult>("simulationresults");
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var scopedServices = scope.ServiceProvider;

    var webSocketService = scopedServices.GetRequiredService<IBitstampWebSocketService>();
    var statisticsService = scopedServices.GetRequiredService<IStatisticsService>();

    Task.Run(() => webSocketService.ConnectAsync(CancellationToken.None));
    Task.Run(() => statisticsService.PrintStatistics());
}

app.Run();
