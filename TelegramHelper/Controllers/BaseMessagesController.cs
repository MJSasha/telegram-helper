using System.Collections.Concurrent;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramHelper.Definitions;
using TelegramHelper.Utils;
using TgBotLib.Core;
using TgBotLib.Core.Base;
using File = System.IO.File;

namespace TelegramHelper.Controllers
{
    public class BaseMessagesController : BotController
    {
        private const string TopicsFilePath = "/app/data/topics.json";
        private static readonly ConcurrentDictionary<long, ConcurrentDictionary<string, ForumTopic>> GroupTopicCache = new();

        private readonly ILogger<BaseMessagesController> _logger;
        private readonly IInlineButtonsGenerationService _buttonsGeneration;

        public BaseMessagesController(ILogger<BaseMessagesController> logger, IInlineButtonsGenerationService buttonsGeneration)
        {
            _logger = logger;
            _buttonsGeneration = buttonsGeneration;
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
                    var tags = text.ExtractTags();

                    foreach (var detectedTag in tags)
                    {
                        var topicTitle = detectedTag.TrimStart('#');

                        var groupTopicCache = GroupTopicCache.GetOrAdd(ChatId, _ => new ConcurrentDictionary<string, ForumTopic>());
                        var topic = groupTopicCache.Values.FirstOrDefault(t => t.Name == topicTitle);

                        if (topic == null)
                        {
                            topic = await CreateTopicAsync(ChatId, topicTitle);
                            await SaveTopicAsync(ChatId, topic);
                        }

                        var sourceTopicName = Update.Message?.ReplyToMessage?.ForumTopicCreated?.Name ?? "General";
                        sourceTopicName = sourceTopicName.Replace(' ', '_');

                        await ForwardMessageToTopic(topic.MessageThreadId, sourceTopicName);
                    }
                }
            }
        }

        private async Task<Dictionary<long, List<ForumTopic>>> GetTopicsFromFileAsync()
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


        private async Task LoadTopicsFromFileAsync()
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

        private async Task SaveTopicAsync(long chatId, ForumTopic topic)
        {
            var groupTopicCache = GroupTopicCache.GetOrAdd(chatId, _ => new ConcurrentDictionary<string, ForumTopic>());
            groupTopicCache[topic.Name] = topic;
            var topicsByGroup = GroupTopicCache.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Values.ToList()
            );
            var json = JsonConvert.SerializeObject(topicsByGroup, Formatting.Indented);
            await File.WriteAllTextAsync(TopicsFilePath, json);
            _logger.LogInformation("Topic {TopicName} created", topic.Name);
        }

        private async Task<ForumTopic> CreateTopicAsync(long chatId, string title)
        {
            var createdTopic = await Client.CreateForumTopicAsync(chatId, title);
            return new ForumTopic
            {
                Name = title,
                MessageThreadId = createdTopic.MessageThreadId
            };
        }

        private async Task ForwardMessageToTopic(int threadId, string sourceTopicName)
        {
            var newText = Update.Message.Text.RemoveTags();

            var topicTag = $"#{sourceTopicName}";

            var newMessageText = $"{topicTag}\n\n{newText}";

            _buttonsGeneration.SetInlineUrlButtons(("Перейти к исходному сообщению", $"https://t.me/c/{ChatId.ToString()[4..]}/{Update.Message.MessageId}"));

            await Client.SendTextMessageAsync(
                chatId: ChatId,
                text: newMessageText,
                messageThreadId: threadId,
                disableNotification: true,
                replyMarkup: _buttonsGeneration.GetButtons()
            );
        }
    }
}