using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramHelper.Definitions;
using TelegramHelper.Domain.Entities;
using TelegramHelper.Infrastructure.Interfaces;
using TelegramHelper.Utils;
using TgBotLib.Core;
using TgBotLib.Core.Base;

namespace TelegramHelper.Controllers;

public class CategoriesController : BotController
{
    private const int PageSize = 10;

    private readonly IInlineButtonsGenerationService _buttonsGenerationService;
    private readonly ICategoriesService _categoriesService;

    public CategoriesController(IInlineButtonsGenerationService buttonsGenerationService, ICategoriesService categoriesService)
    {
        _buttonsGenerationService = buttonsGenerationService;
        _categoriesService = categoriesService;
    }

    [Callback(nameof(DisplayCategories))]
    public async Task DisplayCategories()
    {
        var readResult = await _categoriesService.GetCategories(0, PageSize);
        AddCalegoriesButtons(readResult.Data);

        if (readResult.TotalCount > PageSize * 2)
        {
            _buttonsGenerationService.SetInlineButtons(
                (Messages.Categories.ViewNotes, Messages.Categories.ViewNotes),
                (Messages.Elements.ArrowRight, $"{CallbacksTags.ChannelPagination}:1;")
            );
        }
        else
        {
            _buttonsGenerationService.SetInlineButtons(
                (Messages.Categories.ViewNotes, Messages.Categories.ViewNotes)
            );
        }

        await Client.EditMessageTextAsync(Update.GetChatId(),
            Update.CallbackQuery.Message.MessageId,
            text: Messages.Categories.SelectCategory,
            replyMarkup: (InlineKeyboardMarkup)_buttonsGenerationService.GetButtons()
        );
    }

    [Callback($"{CallbacksTags.ChannelPagination}:(.*?);", isPattern: true)]
    public async Task DisplayPaginatedCategories()
    {
        var page = int.Parse(Update.CallbackQuery.Data.GetTagValue(CallbacksTags.ChannelPagination));
        var readResult = await _categoriesService.GetCategories(page * PageSize, PageSize);
        AddCalegoriesButtons(readResult.Data);

        if (readResult.TotalCount > (page + 1) * PageSize)
        {
            _buttonsGenerationService.SetInlineButtons(
                (Messages.Elements.ArrowLeft, page == 1 ? nameof(DisplayCategories) : $"{CallbacksTags.ChannelPagination}:{page - 1};"),
                (Messages.Categories.ViewNotes, Messages.Categories.ViewNotes),
                (Messages.Elements.ArrowRight, $"{CallbacksTags.ChannelPagination}:{page + 1};")
            );
        }
        else
        {
            _buttonsGenerationService.SetInlineButtons(
                (Messages.Elements.ArrowLeft, page == 1 ? nameof(DisplayCategories) : $"{CallbacksTags.ChannelPagination}:{page - 1};"),
                (Messages.Categories.ViewNotes, Messages.Categories.ViewNotes)
            );
        }

        await Client.EditMessageReplyMarkupAsync(Update.GetChatId(),
            Update.CallbackQuery.Message.MessageId,
            replyMarkup: (InlineKeyboardMarkup?)_buttonsGenerationService.GetButtons()
        );
    }

    private void AddCalegoriesButtons(List<Category> categories)
    {
        for (var i = 0; i < categories.Count; i += 2)
        {
            if (i + 1 <= categories.Count - 1)
            {
                _buttonsGenerationService.SetInlineButtons(categories[i].GetCategoryButton(), categories[i + 1].GetCategoryButton());
            }
            else
            {
                _buttonsGenerationService.SetInlineButtons(categories[i].GetCategoryButton());
            }
        }
    }
}