using TelegramHelper.Interfaces;
using TelegramHelper.Services;
using TgBotLib.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddBotLibCore(Environment.GetEnvironmentVariable("BOT_TOKEN"));

builder.Services.AddSingleton<IForumTopicService, ForumTopicService>();
builder.Services.AddSingleton<IPinnedMessageService, PinnedMessageService>();
builder.Services.AddSingleton<IMessageForwardingService, MessageForwardingService>();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application is starting...");

app.Run();