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
    public Task Start()
    {
        _buttonsGenerationService.SetInlineButtons(
            ("Категории", nameof(CategoriesController.DisplayCategories))
        );

        return Client.SendMdTextMessage(Update.GetChatId(),
            Messages.Base.StartText,
            replyMarkup: _buttonsGenerationService.GetButtons());
    }

    [UnknownUpdate]
    [UnknownMessage]
    public Task UnknownMessage()
    {
        return Client.SendMdTextMessage(Update.GetChatId(),
            Messages.Base.UnknownMessage);
    }
}