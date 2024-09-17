using TgBotLib.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddBotLibCore(Environment.GetEnvironmentVariable("BOT_TOKEN"));

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application is starting...");

app.Run();