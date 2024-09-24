using System.Collections.Concurrent;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramHelper.Interfaces;
using File = System.IO.File;

namespace TelegramHelper.Services
{
    public class ForumTopicService : IForumTopicService
    {
        private const string TopicsFilePath = "/app/data/topics.json";
        private static readonly ConcurrentDictionary<long, ConcurrentDictionary<string, ForumTopic>> GroupTopicCache = new();
        private readonly TelegramBotClient _botClient;

        public ForumTopicService(TelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task<ForumTopic> CreateTopicAsync(long chatId, string title)
        {
            var createdTopic = await _botClient.CreateForumTopicAsync(chatId, title);
            return new ForumTopic
            {
                Name = title,
                MessageThreadId = createdTopic.MessageThreadId
            };
        }

        public async Task SaveTopicAsync(long chatId, ForumTopic topic)
        {
            var groupTopicCache = GroupTopicCache.GetOrAdd(chatId, _ => new ConcurrentDictionary<string, ForumTopic>());
            groupTopicCache[topic.Name] = topic;
            var topicsByGroup = GroupTopicCache.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Values.ToList()
            );
            var json = JsonConvert.SerializeObject(topicsByGroup, Formatting.Indented);
            await File.WriteAllTextAsync(TopicsFilePath, json);
        }

        public async Task<Dictionary<long, List<ForumTopic>>> GetTopicsFromFileAsync()
        {
            var directory = Path.GetDirectoryName(TopicsFilePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(TopicsFilePath))
            {
                return new Dictionary<long, List<ForumTopic>>();
            }

            var json = await File.ReadAllTextAsync(TopicsFilePath);
            return JsonConvert.DeserializeObject<Dictionary<long, List<ForumTopic>>>(json) ?? new Dictionary<long, List<ForumTopic>>();
        }

        public async Task LoadTopicsFromFileAsync()
        {
            var topicsByGroup = await GetTopicsFromFileAsync();
            foreach (var (groupId, topics) in topicsByGroup)
            {
                var groupTopicCache = new ConcurrentDictionary<string, ForumTopic>();
                foreach (var topic in topics)
                {
                    groupTopicCache[topic.Name] = topic;
                }

                GroupTopicCache[groupId] = groupTopicCache;
            }
        }

        public List<ForumTopic> GetTopicsForChat(long chatId)
        {
            return GroupTopicCache.TryGetValue(chatId, out var groupTopicCache)
                ? groupTopicCache.Values.ToList()
                : [];
        }
    }
}