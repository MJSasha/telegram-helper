using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramHelper.Definitions;
using TelegramHelper.Interfaces;
using TelegramHelper.Utils;
using TgBotLib.Core;
using TgBotLib.Core.Base;

namespace TelegramHelper.Controllers
{
    public class BaseMessagesController : BotController
    {
        private readonly IForumTopicService _forumTopicService;
        private readonly IPinnedMessageService _pinnedMessageService;
        private readonly IMessageForwardingService _messageForwardingService;

        public BaseMessagesController(
            IForumTopicService forumTopicService,
            IPinnedMessageService pinnedMessageService,
            IMessageForwardingService messageForwardingService)
        {
            _forumTopicService = forumTopicService;
            _pinnedMessageService = pinnedMessageService;
            _messageForwardingService = messageForwardingService;

            _forumTopicService.LoadTopicsFromFileAsync().Wait();
            _pinnedMessageService.LoadPinnedMessagesFromFileAsync().Wait();
        }

        [Message(Messages.Commands.Start)]
        public async Task Start()
        {
            await Client.SendTextMessageAsync(ChatId, "Ну привет");
        }

        [Message(@"топик .*", isPattern: true)]
        public Task CreateTopic()
        {
            var topicName = Update?.Message?.Text?[4..];
            if (string.IsNullOrWhiteSpace(topicName))
            {
                return Client.SendTextMessageAsync(
                    ChatId,
                    "Какой-то странный топик...",
                    messageThreadId: Update?.Message?.MessageThreadId
                );
            }

            return Client.CreateForumTopicAsync(ChatId, topicName);
        }

        [UnknownUpdate]
        [UnknownMessage]
        public async Task UnknownMessage()
        {
            var message = Update.Message;
            if (message is { Chat.Type: ChatType.Supergroup })
            {
                var tags = message.Entities?.Where(e => e.Type == MessageEntityType.Hashtag)
                    .Select(e => message.Text?.Substring(e.Offset, e.Length))
                    .ToArray() ?? [];

                if (message.Text == null)
                {
                    tags = message.Entities?.Where(e => e.Type == MessageEntityType.Hashtag)
                        .Select(e => message.Caption?.Substring(e.Offset, e.Length))
                        .ToArray() ?? [];
                }

                foreach (var detectedTag in tags.Where(t => !string.IsNullOrWhiteSpace(t)))
                {
                    var topicTitle = detectedTag!.TrimStart('#');

                    var topics = _forumTopicService.GetTopicsForChat(ChatId);
                    var topic = topics.Find(t => t.Name == topicTitle);

                    if (topic == null)
                    {
                        topic = await _forumTopicService.CreateTopicAsync(ChatId, topicTitle);
                        await _forumTopicService.SaveTopicAsync(ChatId, topic);
                    }

                    var sourceTopicName = GetTopicName(Update.Message);
                    sourceTopicName = sourceTopicName.Replace(' ', '_');

                    await _pinnedMessageService.CheckAndUpdatePinnedMessageAsync(ChatId, topic.MessageThreadId, $"#{sourceTopicName}");

                    using var forwardTask = message.Type switch
                    {
                        MessageType.Text => _messageForwardingService.ForwardTextMessageToTopic(ChatId, Update, topic.MessageThreadId, sourceTopicName),
                        MessageType.Photo => _messageForwardingService.ForwardPhotoToTopic(ChatId, Update, topic.MessageThreadId, sourceTopicName),
                        MessageType.Video => _messageForwardingService.ForwardVideoToTopic(ChatId, Update, topic.MessageThreadId, sourceTopicName),
                        MessageType.Document => _messageForwardingService.ForwardDocumentToTopic(ChatId, Update, topic.MessageThreadId, sourceTopicName),
                        _ => Task.CompletedTask
                    };
                    await forwardTask;
                }
            }
        }

        private string GetTopicName(Message? message)
        {
            if (message == null) return "General";
            if (message.ReplyToMessage == null) return message.ReplyToMessage?.ForumTopicCreated?.Name ?? "General";
            return GetTopicName(message.ReplyToMessage);
        }
    }
}