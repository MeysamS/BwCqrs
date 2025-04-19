using Bw.Cqrs.Extensions;
using Bw.Cqrs.InternalCommands.Postgres.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OrderManagement.Application;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Persistence;
using OrderManagement.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson();builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Order Management API", Version = "v1" });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add DbContext
builder.Services.AddScoped<IDbContext,ApplicationDbContext>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Add CQRS
builder.Services.AddBwCqrs(builder =>
{
    builder
        .AddValidation()
        .AddLogging()
        .AddErrorHandling()
        .AddRetry(maxRetries: 3, delayMilliseconds: 1000)
        .AddEventHandling()
        .AddInternalCommands(options =>
        {
            options.MaxRetries = 3;
            options.RetryDelaySeconds = 60;
            options.ProcessingIntervalSeconds = 30;
            options.RetentionDays = 7;
        })
        .UsePostgres(options =>
        {
            options.ConnectionString = connectionString;
            options.CommandTimeout = TimeSpan.FromSeconds(30);
            options.EnableDetailedErrors = true;
            options.EnableSensitiveDataLogging = false;
        });
}, typeof(OrderManagementApplicationInfo).Assembly);

builder.Services.AddAuthorization();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();

    // Execute internal commands table creation script
    var scriptPath = Path.Combine(AppContext.BaseDirectory, "Scripts", "CreateInternalCommandsTable.sql");
    if (File.Exists(scriptPath))
    {
        var script = File.ReadAllText(scriptPath);
        dbContext.Database.ExecuteSqlRaw(script);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Management API V1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
