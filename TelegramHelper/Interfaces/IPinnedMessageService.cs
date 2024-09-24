using TelegramHelper.Controllers;

namespace TelegramHelper.Interfaces;

public interface IPinnedMessageService
{
    Task<Dictionary<int, PinnedMessageInfo>> GetPinnedMessagesFromFileAsync();
    Task LoadPinnedMessagesFromFileAsync();
    Task SavePinnedMessageAsync(int threadId, PinnedMessageInfo pinnedMessageInfo);
    Task CheckAndUpdatePinnedMessageAsync(long chatId, int topicMessageThreadId, string newTag);
}