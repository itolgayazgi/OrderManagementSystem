using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using OrderService.API;
using OrderService.API.Filter;
using OrderService.Application.Features.Orders.Commands.CreateOrder;
using OrderService.Application.Features.Orders.Events;
using OrderService.Application.IntegrationEvents;
using OrderService.Domain.Interfaces;
using OrderService.Domain.IRepository;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Repository;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
  .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
  .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
  .AddEnvironmentVariables();

// DB
builder.Services.AddDbContext<OrderDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Controllers + JSON
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    o.JsonSerializerOptions.DefaultIgnoreCondition =
        System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

// Health checks
var cs = builder.Configuration.GetConnectionString("Default");
builder.Services.AddHealthChecks()
    .AddNpgSql(cs!, name: "postgres",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready", "db" });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Order & Invoice API",
        Version = "v1",
        Description = "Idempotent order + EDA (Outbox/Inbox) demo",
        Contact = new OpenApiContact
        {
            Name = "Ýlyas Tolga Yazgý",
            Email = "i.tolgayazgi@gmail.com"
        }
    });

    // Idempotency Key için global header tanýmý
    opt.AddSecurityDefinition("Idempotency-Key", new OpenApiSecurityScheme
    {
        Description = "Duplicate request prevention. Use a unique UUID for each operation. Expires in 24 hours.",
        Type = SecuritySchemeType.ApiKey,
        Name = "Idempotency-Key",
        In = ParameterLocation.Header,
        Scheme = "apiKey"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Idempotency-Key"
                }
            },
            Array.Empty<string>()
        }
    });

    // XML dokümantasyonu
    var xmlName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlName);
    if (File.Exists(xmlPath))
        opt.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    // Operation ID'leri düzenle (isteðe baðlý)
    opt.CustomOperationIds(apiDesc =>
        apiDesc.TryGetMethodInfo(out MethodInfo methodInfo)
            ? methodInfo.Name
            : null);
});
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IInvoiceGateway, FakeInvoiceGateway>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IdempotencyCheckFilter>();
builder.Services.AddScoped<INotificationHandler<OrderCreatedNotification>, InvoiceCreationHandler>();
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(
        typeof(CreateOrderCommandHandler).Assembly,
        Assembly.GetExecutingAssembly()
    ));
var app = builder.Build();

// **DB migrate + seed**
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.Migrate();
    await DbInitializer.InitializeAsync(db);
}

// **HTTP pipeline**
//if (!app.Environment.IsDevelopment())
//    app.UseHttpsRedirection();          
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order & Invoice API v1");
});

// Health endpoints
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false }); //Baðýmlýlýk dýþý uygulamanýn canlýlýðý
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = r => r.Tags.Contains("ready") }); //Baðýmlýlýklar dahil uygulamanýn hazýr olup olmadýðý

// MVC endpoints
app.MapControllers();

app.Run();