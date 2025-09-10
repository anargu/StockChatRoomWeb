using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StooqBot;

var builder = Host.CreateApplicationBuilder(args);

// Add services
builder.Services.AddHostedService<StooqBotService>();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Build and run the host
var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting StooqBot Worker Service");

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "StooqBot Worker Service failed to start");
    throw;
}
finally
{
    logger.LogInformation("StooqBot Worker Service stopped");
}