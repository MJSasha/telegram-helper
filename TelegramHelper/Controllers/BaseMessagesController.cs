using System.Collections.Concurrent;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramHelper.Definitions;
using TgBotLib.Core;
using TgBotLib.Core.Base;
using File = System.IO.File;

namespace TelegramHelper.Controllers
{
    public class BaseMessagesController : BotController
    {
        private const string TopicsFilePath = "topics.json";
        private static readonly ConcurrentDictionary<string, ForumTopic> TopicCache = new ConcurrentDictionary<string, ForumTopic>();

        public BaseMessagesController()
        {
            LoadTopicsFromFileAsync().Wait();
        }

        [Message(Messages.Commands.Start)]
        public async Task Start()
        {
            await Client.SendTextMessageAsync(ChatId, "Ну привет");
        }

        [UnknownUpdate]
        [UnknownMessage]
        public async Task UnknownMessage()
        {
            var message = Update.Message;
            if (message is { Type: MessageType.Text, Chat.Type: ChatType.Supergroup })
            {
                var text = message.Text;
                if (text != null)
                {
                    var tags = ExtractTags(text);

                    foreach (var detectedTag in tags)
                    {
                        var topicTitle = detectedTag.TrimStart('#');
                        var chatId = message.Chat.Id;

                        var topic = TopicCache.Values.FirstOrDefault(t => t.Name == topicTitle);

                        if (topic == null)
                        {
                            topic = await CreateTopicAsync(topicTitle);
                            await SaveTopicAsync(topic);
                        }

                        await ForwardMessageToTopic(chatId, message.MessageId, topic.MessageThreadId);
                    }
                }
            }
        }

        private static IEnumerable<string> ExtractTags(string text)
        {
            var tags = text.Split(' ')
                .Where(word => word.StartsWith('#'))
                .Distinct();
            return tags;
        }

        private async Task<List<ForumTopic>> GetTopicsFromFileAsync()
        {
            if (!File.Exists(TopicsFilePath))
            {
                return new List<ForumTopic>();
            }

            var json = await File.ReadAllTextAsync(TopicsFilePath);
            return JsonConvert.DeserializeObject<List<ForumTopic>>(json) ?? new List<ForumTopic>();
        }

        private async Task LoadTopicsFromFileAsync()
        {
            var topics = await GetTopicsFromFileAsync();
            foreach (var topic in topics)
            {
                TopicCache[topic.Name] = topic;
            }
        }

        private async Task SaveTopicAsync(ForumTopic topic)
        {
            TopicCache[topic.Name] = topic;
            var topics = TopicCache.Values.ToList();
            var json = JsonConvert.SerializeObject(topics, Formatting.Indented);
            await File.WriteAllTextAsync(TopicsFilePath, json);
        }

        private async Task<ForumTopic> CreateTopicAsync(string title)
        {
            var createdTopic = await Client.CreateForumTopicAsync(ChatId, title);
            return new ForumTopic
            {
                Name = title,
                MessageThreadId = createdTopic.MessageThreadId
            };
        }

        private async Task ForwardMessageToTopic(long chatId, int messageId, int threadId)
        {
            await Client.ForwardMessageAsync(
                chatId: chatId,
                fromChatId: chatId,
                messageId: messageId,
                disableNotification: true,
                messageThreadId: threadId
            );
        }
    }
}