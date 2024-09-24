using Telegram.Bot;
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

        [UnknownUpdate]
        [UnknownMessage]
        public async Task UnknownMessage()
        {
            var message = Update.Message;
            if (message is { Chat.Type: ChatType.Supergroup })
            {
                var tags = message.Text?.ExtractTags() ?? message.Caption?.ExtractTags() ?? Array.Empty<string>();

                foreach (var detectedTag in tags)
                {
                    var topicTitle = detectedTag.TrimStart('#');

                    var topics = _forumTopicService.GetTopicsForChat(ChatId);
                    var topic = topics.Find(t => t.Name == topicTitle);

                    if (topic == null)
                    {
                        topic = await _forumTopicService.CreateTopicAsync(ChatId, topicTitle);
                        await _forumTopicService.SaveTopicAsync(ChatId, topic);
                    }

                    var sourceTopicName = Update.Message?.ReplyToMessage?.ForumTopicCreated?.Name ?? "General";
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
    }
}