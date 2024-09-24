using Telegram.Bot.Types;

namespace TelegramHelper.Interfaces;

public interface IMessageForwardingService
{
    public Task ForwardTextMessageToTopic(long chatId, Update update, int threadId, string sourceTopicName);
    public Task ForwardPhotoToTopic(long chatId, Update update, int threadId, string sourceTopicName);
    public Task ForwardVideoToTopic(long chatId, Update update, int threadId, string sourceTopicName);
    public Task ForwardDocumentToTopic(long chatId, Update update, int threadId, string sourceTopicName);
}