using TelegramHelper.Domain.Entities;
using TelegramHelper.Domain.Models;

namespace TelegramHelper.Infrastructure.Interfaces;

public interface INotesService
{
    Task AddNote(Note note);
    Task<Note> GetNoteById(Guid id, bool includeCategories = false);
    Task<List<Note>> GetByTitlePart(string name, int skip, int take, bool includeCategories = false);
    Task<ReadResult<Note>> GetNotesByCategoryId(Guid id, int skip, int take);
}