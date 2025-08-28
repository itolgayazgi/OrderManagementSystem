using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Persistence;
using OrderService.OutboxWorker.Integrations;
using OrderService.OutboxWorker.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<OrderDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Rabbit publisher (aynı sınıfı kullanıyoruz)
builder.Services.AddSingleton<IRabbitPublisher, RabbitPublisher>();
builder.Services.AddSingleton<IInvoiceGateway, FakeInvoiceGateway>();

// Background işler
builder.Services.AddHostedService<OutboxDispatcher>();     // DB → Rabbit
builder.Services.AddHostedService<OrderCreatedConsumer>(); // Rabbit → İş mantığı (stock+invoice)

var app = builder.Build();
await app.RunAsync();