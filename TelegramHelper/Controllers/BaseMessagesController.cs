using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
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
        return Client.SendTextMessageAsync(Update.GetChatId(),
            Messages.Base.StartText,
            replyMarkup: GetButtons(),
            parseMode: ParseMode.Markdown);
    }

    [Callback("buttonClick")]
    public Task ButtonClicket()
    {
        return Client.SendTextMessageAsync(Update.GetChatId(),
            Messages.Base.ButtonClicked,
            replyMarkup: GetButtons(),
            parseMode: ParseMode.Markdown);
    }

    [UnknownUpdate]
    [UnknownMessage]
    public Task UnknownMessage()
    {
        return Client.SendTextMessageAsync(Update.GetChatId(),
            Messages.Base.UnknownMessage,
            replyMarkup: GetButtons(),
            parseMode: ParseMode.Markdown);
    }

    private IReplyMarkup GetButtons()
    {
        _buttonsGenerationService.SetInlineButtons(("Кнопочка", "buttonClick"), ("Кнопочка", "buttonClick"));
        _buttonsGenerationService.SetInlineButtons(("Ух.. Кнопочка", "buttonClick"));
        return _buttonsGenerationService.GetButtons();
    }
}