using TgBotLib.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBotLibCore(Environment.GetEnvironmentVariable("BOT_TOKEN"));

var app = builder.Build();
app.Run();