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

    [Message("/start")]
    public async Task Start()
    {
        await Client.DeleteMessageAsync(Update.GetChatId(), Update.Message.MessageId);

        _buttonsGenerationService.SetInlineButtons(
            ("Категории", nameof(CategoriesController.DisplayCategories))
        );

        await Client.SendMdTextMessage(Update.GetChatId(),
            Messages.Base.StartText,
            replyMarkup: _buttonsGenerationService.GetButtons());
    }

    [UnknownUpdate]
    [UnknownMessage]
    public Task UnknownMessage()
    {
        if (Update.Message?.ViaBot != null) return Task.CompletedTask;
        return Client.SendMdTextMessage(Update.GetChatId(), Messages.Base.UnknownMessage);
    }
}