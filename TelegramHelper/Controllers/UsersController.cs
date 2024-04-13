using Telegram.Bot;
using TelegramHelper.Definitions;
using TelegramHelper.Domain.Entities;
using TelegramHelper.Infrastructure;
using TgBotLib.Core;
using TgBotLib.Core.Base;

namespace TelegramHelper.Controllers;

public class UsersController : BotController
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    [Message("/reg")]
    public async Task StartRegistration()
    {
        var user = new User
        {
            ChatId = Update.GetChatId(),
            FirstName = Update.Message.From.FirstName,
            Username = Update.Message.From.Username,
            LanguageCode = Update.Message.From.LanguageCode
        };
        await _usersService.AddUser(user);
        await Client.SendTextMessageAsync(Update.GetChatId(), string.Format(Messages.Users.YouAreRegistered, Update.GetChatId()));
    }
}