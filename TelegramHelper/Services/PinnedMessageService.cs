using System.Collections.Concurrent;
using Newtonsoft.Json;
using Telegram.Bot;
using TelegramHelper.Controllers;
using TelegramHelper.Interfaces;

namespace TelegramHelper.Services;

public class PinnedMessageService : IPinnedMessageService
{
    private const string PinnedMessagesFilePath = "/app/data/pinned_messages.json";
    private static readonly ConcurrentDictionary<int, PinnedMessageInfo> PinnedMessageCache = new();
    private readonly TelegramBotClient _botClient;

    public PinnedMessageService(TelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task<Dictionary<int, PinnedMessageInfo>> GetPinnedMessagesFromFileAsync()
    {
        var directory = Path.GetDirectoryName(PinnedMessagesFilePath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!File.Exists(PinnedMessagesFilePath))
        {
            return new Dictionary<int, PinnedMessageInfo>();
        }

        var json = await File.ReadAllTextAsync(PinnedMessagesFilePath);
        return JsonConvert.DeserializeObject<Dictionary<int, PinnedMessageInfo>>(json) ?? new Dictionary<int, PinnedMessageInfo>();
    }

    public async Task LoadPinnedMessagesFromFileAsync()
    {
        var pinnedMessages = await GetPinnedMessagesFromFileAsync();
        foreach (var (threadId, pinnedMessageInfo) in pinnedMessages)
        {
            PinnedMessageCache[threadId] = pinnedMessageInfo;
        }
    }

    public async Task SavePinnedMessageAsync(int threadId, PinnedMessageInfo pinnedMessageInfo)
    {
        PinnedMessageCache[threadId] = pinnedMessageInfo;
        var pinnedMessages = PinnedMessageCache.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value
        );
        var json = JsonConvert.SerializeObject(pinnedMessages, Formatting.Indented);
        await File.WriteAllTextAsync(PinnedMessagesFilePath, json);
    }

    public async Task CheckAndUpdatePinnedMessageAsync(long chatId, int topicMessageThreadId, string newTag)
    {
        var pinnedMessageInfo = PinnedMessageCache.GetOrAdd(topicMessageThreadId, _ => new PinnedMessageInfo());

        if (pinnedMessageInfo.MessageId == 0)
        {
            var messageText = $"Список тегов:\n\n▪️ {newTag}";
            var message = await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: messageText,
                messageThreadId: topicMessageThreadId
            );

            pinnedMessageInfo.MessageId = message.MessageId;
            pinnedMessageInfo.Text = messageText;

            await _botClient.PinChatMessageAsync(chatId, message.MessageId);
            await SavePinnedMessageAsync(topicMessageThreadId, pinnedMessageInfo);
        }
        else
        {
            if (!pinnedMessageInfo.Text!.Contains(newTag))
            {
                var updatedText = pinnedMessageInfo.Text + $"\n▪️ {newTag}";

                await _botClient.EditMessageTextAsync(
                    chatId: chatId,
                    messageId: pinnedMessageInfo.MessageId,
                    text: updatedText
                );

                pinnedMessageInfo.Text = updatedText;
                await SavePinnedMessageAsync(topicMessageThreadId, pinnedMessageInfo);
            }
        }
    }
}