using Telegram.Bot.Types;

namespace TelegramHelper.Interfaces;

public interface IForumTopicService
{
    Task<ForumTopic> CreateTopicAsync(long chatId, string title);
    Task SaveTopicAsync(long chatId, ForumTopic topic);
    Task<Dictionary<long, List<ForumTopic>>> GetTopicsFromFileAsync();
    Task LoadTopicsFromFileAsync();
    List<ForumTopic> GetTopicsForChat(long chatId);
}