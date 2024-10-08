using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramHelper.Definitions;
using TelegramHelper.Interfaces;
using TelegramHelper.Utils;
using TgBotLib.Core;

namespace TelegramHelper.Services;

public class MessageForwardingService : IMessageForwardingService
{
    private readonly TelegramBotClient _botClient;
    private readonly IServiceProvider _serviceProvider;

    public MessageForwardingService(TelegramBotClient botClient, IServiceProvider serviceProvider)
    {
        _botClient = botClient;
        _serviceProvider = serviceProvider;
    }

    public async Task ForwardTextMessageToTopic(long chatId, Update update, int threadId, string sourceTopicName)
    {
        var newText = update.Message!.Text!.RemoveTags();

        var topicTag = $"#{sourceTopicName}";

        var newMessageText = $"{topicTag}\n\n{newText}";

        var buttonsGeneration = SetGoToButton(chatId, update.Message.MessageId);
        await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: newMessageText,
            messageThreadId: threadId,
            disableNotification: true,
            replyMarkup: buttonsGeneration.GetButtons()
        );
    }

    public async Task ForwardPhotoToTopic(long chatId, Update update, int threadId, string sourceTopicName)
    {
        var topicTag = $"#{sourceTopicName}";
        var originalText = update.Message!.Caption?.RemoveTags() ?? string.Empty;

        var inputMediaPhoto = new InputMediaPhoto(new InputFileId(update.Message!.Photo!.Last().FileId))
        {
            Caption = $"{topicTag}\n\n{originalText}"
        };

        await _botClient.SendMediaGroupAsync(
            chatId: chatId,
            media: new[] { inputMediaPhoto },
            messageThreadId: threadId,
            disableNotification: true
        );
    }

    public async Task ForwardVideoToTopic(long chatId, Update update, int threadId, string sourceTopicName)
    {
        var video = update.Message!.Video!;
        var topicTag = $"#{sourceTopicName}";
        var originalText = update.Message!.Caption?.RemoveTags() ?? string.Empty;

        var buttonsGeneration = SetGoToButton(chatId, update.Message.MessageId);
        await _botClient.SendVideoAsync(
            chatId: chatId,
            video: new InputFileId(video.FileId),
            caption: $"{topicTag}\n\n{originalText}",
            messageThreadId: threadId,
            disableNotification: true,
            replyMarkup: buttonsGeneration.GetButtons()
        );
    }

    public async Task ForwardDocumentToTopic(long chatId, Update update, int threadId, string sourceTopicName)
    {
        var document = update.Message!.Document!;
        var topicTag = $"#{sourceTopicName}";
        var originalText = update.Message!.Caption?.RemoveTags() ?? string.Empty;

        var buttonsGeneration = SetGoToButton(chatId, update.Message.MessageId);
        await _botClient.SendDocumentAsync(
            chatId: chatId,
            document: new InputFileId(document.FileId),
            caption: $"{topicTag}\n\n{originalText}",
            messageThreadId: threadId,
            disableNotification: true,
            replyMarkup: buttonsGeneration.GetButtons()
        );
    }

    private IInlineButtonsGenerationService SetGoToButton(long chatId, long messageId)
    {
        var buttonsGeneration = _serviceProvider.GetRequiredService<IInlineButtonsGenerationService>();
        buttonsGeneration.SetInlineUrlButtons((Messages.Buttons.GoToSource, $"https://t.me/c/{chatId.ToString()[4..]}/{messageId}"));
        return buttonsGeneration;
    }
}