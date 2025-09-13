using Microsoft.EntityFrameworkCore;
using OrderService.Application.Features.Orders.Events;
using OrderService.Domain.Interfaces;
using OrderService.Domain.IRepository;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Repository;
using OrderService.OutboxWorker.Integrations;
using OrderService.OutboxWorker.Messaging;

var builder = Host.CreateApplicationBuilder(args);


builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables(); //docker veya local için

builder.Services.AddDbContext<OrderDbContext>(o =>  
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Rabbit publisher (aynı sınıfı kullanıyoruz)
builder.Services.AddSingleton<IRabbitPublisher, RabbitPublisher>();
builder.Services.AddSingleton<IInvoiceGateway, HttpFakeInvoiceGateway>();
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(StockDeductionHandler).Assembly));
// Background işler
builder.Services.AddHostedService<OutboxDispatcher>();     // DB → Rabbit
builder.Services.AddHostedService<OrderCreatedConsumer>(); // Rabbit → İş mantığı (stock+invoice)

var app = builder.Build();
await app.RunAsync();