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

public class NotesController : BotController
{
    private const int PageSize = 10;

    private readonly IInlineButtonsGenerationService _buttonsGenerationService;
    private readonly IUsersActionsService _usersActionsService;
    private readonly INotesService _notesService;
    private readonly ICategoriesService _categoriesService;

    public NotesController(IInlineButtonsGenerationService buttonsGenerationService, IUsersActionsService usersActionsService, INotesService notesService, ICategoriesService categoriesService)
    {
        _buttonsGenerationService = buttonsGenerationService;
        _usersActionsService = usersActionsService;
        _notesService = notesService;
        _categoriesService = categoriesService;
    }

    [Callback($"{nameof(DisplayNotes)}:(.*?);", isPattern: true)]
    [Callback($"{nameof(DisplayNotes)}:(.*?);{CallbacksTags.Pagination}:(.*?);", isPattern: true)]
    public async Task DisplayNotes()
    {
        var categoryId = new Guid(Update.CallbackQuery.Data.GetTagValue(nameof(DisplayNotes)));
        var page = Update.CallbackQuery.Data.GetTagValue(CallbacksTags.Pagination) ?? "0";
        var pageNumber = int.Parse(page);
        var category = await _categoriesService.GetCategoryById(categoryId);
        var readResult = await _notesService.GetNotesByCategoryId(categoryId, pageNumber * PageSize, PageSize);

        AddNotesButtons(readResult.Data);
        AddPaginationButtons(pageNumber, readResult);
        AddGoBackButton(category.Id);

        await Client.EditMessageTextAsync(
            Update.GetChatId(),
            Update.CallbackQuery.Message.MessageId,
            text: category.Name,
            replyMarkup: (InlineKeyboardMarkup)_buttonsGenerationService.GetButtons()
        );
    }

    private void AddGoBackButton(Guid categoryId)
    {
        _buttonsGenerationService.SetInlineButtons(
            (Messages.Elements.GoBack, $"{CallbacksTags.ParentCategory}:{categoryId};")
        );
    }

    private void AddPaginationButtons(int pageNumber, ReadResult<Note> readResult)
    {
        List<(string, string)> buttonsMarkup = [];
        if (pageNumber > 0)
        {
            buttonsMarkup.Add((Messages.Elements.ArrowLeft, $"{CallbacksTags.Pagination}:{pageNumber - 1};"));
        }

        if (readResult.TotalCount > (pageNumber + 1) * PageSize)
        {
            buttonsMarkup.Add((Messages.Elements.ArrowRight, $"{CallbacksTags.Pagination}:{pageNumber + 1};"));
        }

        _buttonsGenerationService.SetInlineButtons(buttonsMarkup.ToArray());
    }

    private void AddNotesButtons(List<Note> categories)
    {
        for (var i = 0; i < categories.Count; i += 2)
        {
            if (i + 1 <= categories.Count - 1)
            {
                _buttonsGenerationService.SetInlineButtons(categories[i].GetNoteButton(), categories[i + 1].GetNoteButton());
            }
            else
            {
                _buttonsGenerationService.SetInlineButtons(categories[i].GetNoteButton());
            }
        }
    }
}