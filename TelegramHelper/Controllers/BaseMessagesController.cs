using Telegram.Bot;
using TelegramHelper.Definitions;
using TgBotLib.Core;
using TgBotLib.Core.Base;

namespace TelegramHelper.Controllers;

public class BaseMessagesController : BotController
{
    private readonly IInlineButtonsGenerationService _buttonsGenerationService;

    public BaseMessagesController(IInlineButtonsGenerationService buttonsGenerationService)
    {
        _buttonsGenerationService = buttonsGenerationService;
    }

    [Message(Messages.Commands.Start)]
    public async Task Start()
    {
        await Client.SendTextMessageAsync(ChatId, "Ну привет");
    }

    [UnknownUpdate]
    [UnknownMessage]
    public Task UnknownMessage()
    {
        if (Update.Message?.ViaBot != null) return Task.CompletedTask;
        return Client.SendMdTextMessage(ChatId, Messages.Base.UnknownMessage);
    }
}