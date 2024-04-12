using Microsoft.EntityFrameworkCore;
using TelegramHelper.Infrastructure;
using TgBotLib.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddBotLibCore(Environment.GetEnvironmentVariable("BOT_TOKEN"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

app.Run();