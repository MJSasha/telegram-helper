using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramHelper.Definitions;
using TelegramHelper.Domain.Entities;
using TelegramHelper.Domain.Models;
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
    [Callback($"{CallbacksTags.CategoryPagination}:(.*?);", isPattern: true)]
    [Callback($"{CallbacksTags.ParentCategory}:(.*?);", isPattern: true)]
    [Callback($"{CallbacksTags.CategoryPagination}:(.*?);{CallbacksTags.ParentCategory}:(.*?);", isPattern: true)]
    public async Task DisplayCategories()
    {
        var showForParent = Guid.TryParse(Update.CallbackQuery.Data.GetTagValue(CallbacksTags.ParentCategory), out var parentCategoryId);
        var page = Update.CallbackQuery.Data.GetTagValue(CallbacksTags.CategoryPagination) ?? "0";
        var pageNumber = int.Parse(page);
        ReadResult<Category> readResult;
        Category? parentCategory = null;

        if (!showForParent)
        {
            readResult = await _categoriesService.GetCategories(pageNumber * PageSize, PageSize);
        }
        else
        {
            parentCategory = await _categoriesService.GetCategoryById(parentCategoryId);
            readResult = await _categoriesService.GetSubCategories(parentCategoryId, pageNumber * PageSize, PageSize);
        }

        AddCategoriesButtons(readResult.Data);

        List<(string, string)> buttonsMarkup = [];
        if (pageNumber > 0)
        {
            buttonsMarkup.Add((Messages.Elements.ArrowLeft, $"{CallbacksTags.CategoryPagination}:{pageNumber - 1};"));
        }

        buttonsMarkup.Add((Messages.Categories.ViewNotes, Messages.Categories.ViewNotes));

        if (readResult.TotalCount > (pageNumber + 1) * PageSize)
        {
            buttonsMarkup.Add((Messages.Elements.ArrowRight, $"{CallbacksTags.CategoryPagination}:{pageNumber + 1};"));
        }

        _buttonsGenerationService.SetInlineButtons(buttonsMarkup.ToArray());

        await SendCategoriesPaginationList(parentCategory);
    }

    private async Task SendCategoriesPaginationList(Category? parentCategory)
    {
        if (Update.CallbackQuery.Data.Equals(nameof(DisplayCategories)))
        {
            await Client.EditMessageTextAsync(Update.GetChatId(),
                Update.CallbackQuery.Message.MessageId,
                text: Messages.Categories.SelectCategory,
                replyMarkup: (InlineKeyboardMarkup)_buttonsGenerationService.GetButtons()
            );
        }
        else if (parentCategory != null)
        {
            await Client.EditMessageTextAsync(Update.GetChatId(),
                Update.CallbackQuery.Message.MessageId,
                text: parentCategory.Name,
                replyMarkup: (InlineKeyboardMarkup)_buttonsGenerationService.GetButtons()
            );
        }
        else
        {
            await Client.EditMessageReplyMarkupAsync(Update.GetChatId(),
                Update.CallbackQuery.Message.MessageId,
                replyMarkup: (InlineKeyboardMarkup?)_buttonsGenerationService.GetButtons()
            );
        }
    }

    private void AddCategoriesButtons(List<Category> categories)
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