using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramHelper.Definitions;
using TelegramHelper.Domain.Entities;
using TelegramHelper.Domain.Enums;
using TelegramHelper.Domain.Models;
using TelegramHelper.Infrastructure;
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
    private readonly IUsersActionsService _usersActionsService;
    private readonly IUsersService _usersService;

    public CategoriesController(IInlineButtonsGenerationService buttonsGenerationService, ICategoriesService categoriesService, IUsersActionsService usersActionsService, IUsersService usersService)
    {
        _buttonsGenerationService = buttonsGenerationService;
        _categoriesService = categoriesService;
        _usersActionsService = usersActionsService;
        _usersService = usersService;
    }

    [Callback(nameof(DisplayCategories))]
    [Callback($"{CallbacksTags.Pagination}:(.*?);", isPattern: true)]
    [Callback($"{CallbacksTags.ParentCategory}:(.*?);", isPattern: true)]
    [Callback($"{CallbacksTags.Pagination}:(.*?);{CallbacksTags.ParentCategory}:(.*?);", isPattern: true)]
    public async Task DisplayCategories()
    {
        var showForParent = Guid.TryParse(Update.CallbackQuery.Data.GetTagValue(CallbacksTags.ParentCategory), out var parentCategoryId);
        var page = Update.CallbackQuery.Data.GetTagValue(CallbacksTags.Pagination) ?? "0";
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
        AddPaginationButtons(pageNumber, readResult, showForParent ? parentCategoryId : null);
        AddGoBackButton(parentCategory);
        await AddCreateButton(parentCategory);

        await SendCategoriesPaginationList(parentCategory);
    }

    [Callback(nameof(StartCreatingCategory))]
    [Callback($"{nameof(StartCreatingCategory)}:(.*?);", isPattern: true)]
    public async Task StartCreatingCategory()
    {
        var currentUser = await _usersService.GetUser(Update.GetChatId());
        if (currentUser?.Role is UserRole.Admin or UserRole.Editor)
        {
            var category = new Category();
            var parentCategoryId = Update.CallbackQuery.Data.GetTagValue(CallbacksTags.ParentCategory);
            if (!string.IsNullOrWhiteSpace(parentCategoryId)) category.ParentCategoryId = Guid.Parse(parentCategoryId);

            _usersActionsService.HandleUser(Update.GetChatId(), nameof(StartCreatingCategory), category);

            await Client.EditMessageTextAsync(Update.GetChatId(),
                Update.CallbackQuery.Message.MessageId,
                Messages.Categories.EnterCategoryName
            );
        }
    }

    [ActionStep(nameof(StartCreatingCategory), step: 0)]
    public async Task CompleteCategoryCreating()
    {
        var currentUser = await _usersService.GetUser(Update.GetChatId());
        if (currentUser?.Role is UserRole.Admin or UserRole.Editor)
        {
            var category = _usersActionsService.GetUserActionStepInfo(Update.GetChatId()).GetPayload<Category>();
            category.Name = Update.GetMessageText();
            await _categoriesService.AddCategory(category);

            _buttonsGenerationService.SetInlineButtons(category.GetCategoryButton());

            await Client.SendTextMessageAsync(Update.GetChatId(),
                Messages.Categories.CategoryCreated,
                replyMarkup: _buttonsGenerationService.GetButtons()
            );
        }
    }

    private async Task AddCreateButton(Category? parentCategory)
    {
        var currentUser = await _usersService.GetUser(Update.GetChatId());
        if (currentUser?.Role is UserRole.Admin or UserRole.Editor)
        {
            _buttonsGenerationService.SetInlineButtons(
                (Messages.Elements.Add, parentCategory != null
                    ? $"{nameof(StartCreatingCategory)}:{parentCategory.Id};"
                    : nameof(StartCreatingCategory))
            );
        }
    }

    private void AddGoBackButton(Category? parentCategory)
    {
        if (parentCategory != null)
        {
            _buttonsGenerationService.SetInlineButtons(
                (Messages.Elements.GoBack, parentCategory.ParentCategoryId == null
                    ? nameof(DisplayCategories)
                    : $"{CallbacksTags.ParentCategory}:{parentCategory.ParentCategoryId};")
            );
        }
    }

    private void AddPaginationButtons(int pageNumber, ReadResult<Category> readResult, Guid? parentCategoryId)
    {
        var parentCategoryTag = $"{(parentCategoryId != default ? $"{CallbacksTags.ParentCategory}:{parentCategoryId};" : string.Empty)}";

        List<(string, string)> buttonsMarkup = [];
        if (pageNumber > 0)
        {
            buttonsMarkup.Add((Messages.Elements.ArrowLeft,
                $"{CallbacksTags.Pagination}:{pageNumber - 1};{parentCategoryTag}"));
        }

        if (parentCategoryId != default)
        {
            buttonsMarkup.Add(
                (Messages.Categories.ViewNotes, $"{nameof(NotesController.DisplayNotes)}:{parentCategoryId};")
            );
        }

        if (readResult.TotalCount > (pageNumber + 1) * PageSize)
        {
            buttonsMarkup.Add((Messages.Elements.ArrowRight,
                $"{CallbacksTags.Pagination}:{pageNumber + 1};{parentCategoryTag}"));
        }

        _buttonsGenerationService.SetInlineButtons(buttonsMarkup.ToArray());
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