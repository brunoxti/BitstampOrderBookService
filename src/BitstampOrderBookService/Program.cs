using BitstampOrderBookService.Application.Interfaces;
using BitstampOrderBookService.Application.Services;
using BitstampOrderBookService.Domain.ValueObjects;
using BitstampOrderBookService.Infrastructure.Data;
using BitstampOrderBookService.Infrastructure.Repository;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<ApplicationDbContext>();
builder.Services.AddSingleton<BitstampWebSocketService>();
builder.Services.AddSingleton<StatisticsService>();
builder.Services.AddSingleton<PricingService>();

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection(nameof(MongoDbSettings)));

builder.Services.Configure<WebSocketSettings>(
    builder.Configuration.GetSection(nameof(WebSocketSettings)));

builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var client = new MongoClient(settings.ConnectionString);
    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.AddSingleton<ApplicationDbContext>();
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

var webSocketService = app.Services.GetRequiredService<BitstampWebSocketService>();
var statisticsService = app.Services.GetRequiredService<StatisticsService>();
var webSocketSettings = app.Services.GetRequiredService<IOptions<WebSocketSettings>>().Value;

Task.Run(() => webSocketService.ConnectAsync(webSocketSettings.Instruments));
Task.Run(() => statisticsService.PrintStatistics());

app.Run();
