using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramHelper.Definitions;
using TgBotLib.Core;
using TgBotLib.Core.Base;

namespace TelegramHelper.Controllers;

public class BaseMessagesController : BotController
{
    private readonly IInlineButtonsGenerationService _buttonsGenerationService;

    private readonly Dictionary<string, string> _commands = new()
    {
        { "Расскажи о себе", "Я бот, который любит кнопочки!" },
        { "А что ты умеешь?", "Я умею отправлять кнопочки... Умею отвечать на кнопочки!" }
    };

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
    public Task ButtonClicked()
    {
        return Client.SendTextMessageAsync(Update.GetChatId(),
            Messages.Base.ButtonClicked,
            replyMarkup: GetButtons(),
            parseMode: ParseMode.Markdown);
    }

    [InlineQuery]
    public Task TestInlineQuery()
    {
        var results = new List<InlineQueryResult>();

        var counter = 0;
        foreach (var command in _commands.Keys.Where(s => s.ToLower().Contains(Update.InlineQuery.Query.ToLower())))
        {
            results.Add(new InlineQueryResultArticle($"{counter}", command, new InputTextMessageContent(_commands[command])));
            counter++;
        }

        return Client.AnswerInlineQueryAsync(Update.InlineQuery.Id, results);
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