using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramHelper.Definitions;
using TelegramHelper.Domain.Entities;
using TelegramHelper.Domain.Models;
using TelegramHelper.Infrastructure;
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
    private readonly IUsersService _usersService;

    public NotesController(IInlineButtonsGenerationService buttonsGenerationService, IUsersActionsService usersActionsService, INotesService notesService, ICategoriesService categoriesService, IUsersService usersService)
    {
        _buttonsGenerationService = buttonsGenerationService;
        _usersActionsService = usersActionsService;
        _notesService = notesService;
        _categoriesService = categoriesService;
        _usersService = usersService;
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

    [Callback($"{CallbacksTags.Note}:(.*?);", isPattern: true)]
    public async Task ShowNote()
    {
        var noteId = new Guid(Update.CallbackQuery.Data.GetTagValue(CallbacksTags.Note));
        var note = await _notesService.GetNoteById(noteId, includeCategories: true);

        var message = string.Format(Messages.Notes.NoteTemplate, note.Title, note.Content, GetCategoryHierarchy(note.Category));

        Client.SendMdTextMessage(Update.GetChatId(), message);
    }


    [Callback($"{nameof(StartNoteCreating)}:(.*?);", isPattern: true)]
    public async Task StartNoteCreating()
    {
        var currentUser = await _usersService.GetUser(Update.GetChatId());
        if (_usersService.CheckUserCanEdit(currentUser))
        {
            var categoryId = new Guid(Update.CallbackQuery.Data.GetTagValue(nameof(StartNoteCreating)));
            var note = new Note
            {
                CategoryId = categoryId,
                AuthorId = currentUser.Id
            };

            _usersActionsService.HandleUser(Update.GetChatId(), nameof(StartNoteCreating), note);

            await Client.SendTextMessageAsync(Update.GetChatId(), Messages.Notes.EnterNoteTitle);
        }
    }

    [ActionStep(nameof(StartNoteCreating), step: 0)]
    public async Task AddNoteTitle()
    {
        var currentUser = await _usersService.GetUser(Update.GetChatId());
        if (_usersService.CheckUserCanEdit(currentUser))
        {
            var note = _usersActionsService.GetUserActionStepInfo(Update.GetChatId()).GetPayload<Note>();
            note.Title = Update.GetMessageText();

            await Client.SendTextMessageAsync(Update.GetChatId(), Messages.Notes.EnterNoteText);
        }
    }

    [ActionStep(nameof(StartNoteCreating), step: 1)]
    public async Task CompleteNoteCreating()
    {
        var currentUser = await _usersService.GetUser(Update.GetChatId());
        if (_usersService.CheckUserCanEdit(currentUser))
        {
            var note = _usersActionsService.GetUserActionStepInfo(Update.GetChatId()).GetPayload<Note>();
            note.Content = Update.GetMessageText();
            await _notesService.AddNote(note);

            _buttonsGenerationService.SetInlineButtons(note.GetNoteButton());
            await Client.SendTextMessageAsync(Update.GetChatId(),
                Messages.Notes.NoteCreated,
                replyMarkup: _buttonsGenerationService.GetButtons()
            );
        }
    }

    private string GetCategoryHierarchy(Category category)
    {
        if (category == null) return string.Empty;

        var categoryHierarchy = new List<string>();
        BuildCategoryHierarchy(category, categoryHierarchy);
        categoryHierarchy.Reverse();

        return string.Join(" ➡ ", categoryHierarchy);
    }

    private void BuildCategoryHierarchy(Category category, List<string> hierarchy)
    {
        if (category == null) return;

        hierarchy.Add(category.Name);
        BuildCategoryHierarchy(category.ParentCategory, hierarchy);
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