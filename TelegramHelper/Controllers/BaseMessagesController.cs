using Telegram.Bot;
using TgBotLib.Core;
using TgBotLib.Core.Base;

namespace TelegramHelper.Controllers;

public class BaseMessagesController : BotController
{
    [Message("/start")]
    public Task Start()
    {
        return Client.SendTextMessageAsync(BotContext.Update.GetChatId(), "Hello world!");
    }
}