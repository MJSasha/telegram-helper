using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramHelper.Definitions;
using TelegramHelper.Domain.Entities;
using TelegramHelper.Domain.Models;
using TelegramHelper.Infrastructure;
using TelegramHelper.Infrastructure.Interfaces;
using TelegramHelper.Utils;
using TelegramHelper.Utils.Helpers;
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
        var showForParent = Guid.TryParse(InputText.GetTagValue(CallbacksTags.ParentCategory), out var parentCategoryId);
        var page = InputText.GetTagValue(CallbacksTags.Pagination) ?? "0";
        var pageNumber = int.Parse(page);
        ReadResult<Category> readResult;
        Category? parentCategory = null;

        if (!showForParent)
        {
            readResult = await _categoriesService.GetCategories(pageNumber * PageSize, PageSize);
        }
        else
        {
            parentCategory = await _categoriesService.GetCategoryById(parentCategoryId, includeParent: true);
            readResult = await _categoriesService.GetSubCategories(parentCategoryId, pageNumber * PageSize, PageSize);
        }

        AddCategoriesButtons(readResult.Data);
        AddPaginationButtons(pageNumber, readResult, showForParent ? parentCategoryId : null);
        AddGoBackButton(parentCategory);
        await AddRedactionButtons(parentCategory);

        await SendCategoriesPaginationList(parentCategory);
    }

    [Callback(nameof(StartCreatingCategory))]
    [Callback($"{nameof(StartCreatingCategory)}:(.*?);", isPattern: true)]
    public async Task StartCreatingCategory()
    {
        if (await _usersService.CheckUserCanEdit(ChatId))
        {
            var category = new Category();
            var parentCategoryId = InputText.GetTagValue(nameof(StartCreatingCategory));
            if (!string.IsNullOrWhiteSpace(parentCategoryId)) category.ParentCategoryId = Guid.Parse(parentCategoryId);

            _usersActionsService.HandleUser(ChatId, nameof(StartCreatingCategory), category);

            await Client.EditMessageTextAsync(ChatId,
                Update.CallbackQuery.Message.MessageId,
                Messages.Categories.EnterCategoryName
            );
        }
    }

    [ActionStep(nameof(StartCreatingCategory), step: 0)]
    public async Task CompleteCategoryCreating()
    {
        if (await _usersService.CheckUserCanEdit(ChatId))
        {
            var category = _usersActionsService.GetUserActionStepInfo(ChatId).GetPayload<Category>();
            var inputMessageText = InputText;

            if (inputMessageText.Equals(Messages.Commands.Cancel))
            {
                await Client.SendTextMessageAsync(ChatId, Messages.Base.Canceled);
            }
            else
            {
                category.Name = inputMessageText;
                await _categoriesService.AddCategory(category);

                _buttonsGenerationService.SetInlineButtons(category.GetCategoryButton());

                await Client.SendTextMessageAsync(ChatId,
                    Messages.Categories.CategoryCreated,
                    replyMarkup: _buttonsGenerationService.GetButtons()
                );
            }
        }
    }

    [Callback($"{nameof(DeleteCategory)}:(.*?);", isPattern: true)]
    public async Task DeleteCategory()
    {
        if (await _usersService.CheckUserCanDelete(ChatId))
        {
            var categoryId = new Guid(InputText.GetTagValue(nameof(DeleteCategory)));
            await _categoriesService.Delete(categoryId);
            await Client.SendTextMessageAsync(ChatId, Messages.Base.Deleted);
        }
    }

    private async Task AddRedactionButtons(Category? parentCategory)
    {
        if (await _usersService.CheckUserCanEdit(ChatId))
        {
            List<(string, string)> markup = [];
            if (parentCategory != null)
            {
                markup.Add((Messages.Elements.AddCategory, $"{nameof(StartCreatingCategory)}:{parentCategory.Id};"));

                if (await _usersService.CheckUserCanDelete(ChatId))
                {
                    markup.Add((Messages.Elements.Delete, $"{nameof(DeleteCategory)}:{parentCategory.Id};"));
                }

                markup.Add((Messages.Elements.AddNote, $"{nameof(NotesController.StartNoteCreating)}:{parentCategory.Id};"));
            }
            else
            {
                markup.Add((Messages.Elements.AddCategory, nameof(StartCreatingCategory)));
            }

            _buttonsGenerationService.SetInlineButtons(markup.ToArray());
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
        if (InputText.Equals(nameof(DisplayCategories)))
        {
            await Client.EditMessageTextAsync(ChatId,
                Update.CallbackQuery.Message.MessageId,
                text: Messages.Categories.SelectCategory,
                replyMarkup: (InlineKeyboardMarkup)_buttonsGenerationService.GetButtons(),
                parseMode: ParseMode.MarkdownV2
            );
        }
        else if (parentCategory != null)
        {
            var message = string.Format(Messages.Categories.CategoryTemplate, MessageFormatHelper.GetCategoryHierarchy(parentCategory));
            await Client.EditMessageTextAsync(ChatId,
                Update.CallbackQuery.Message.MessageId,
                text: message.EscapeMarkdownSpecialCharacters(),
                replyMarkup: (InlineKeyboardMarkup)_buttonsGenerationService.GetButtons(),
                parseMode: ParseMode.MarkdownV2
            );
        }
        else
        {
            await Client.EditMessageReplyMarkupAsync(ChatId,
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