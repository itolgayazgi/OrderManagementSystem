using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using OrderService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<OrderDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
var cs = builder.Configuration.GetConnectionString("Default");

builder.Services.AddHealthChecks()
    // DB hazýr mý? 'ready' etiketi ile iþaretliyoruz
    .AddNpgSql(
        cs!,
        name: "postgres",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready", "db" }
    );
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Order & Invoice API",
        Version = "v1",
        Description = "Idempotent sipariþ oluþturma + EDA (Outbox/Inbox) demosu"
    });
});

var app = builder.Build();
// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order & Invoice API v1"));

// HEALTH ENDPOINTLERÝ
// Liveness: sadece uygulama ayakta mý (baðýmlýlýk kontrolü yapmaz)
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

// Readiness: 'ready' etiketi taþýyan kontroller (Postgres vs.)
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready")
});
// *** Auto-migrate: API açýlýrken DB'ye migration uygular ***
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.Migrate();
    await DbInitializer.InitializeAsync(db);
}
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
