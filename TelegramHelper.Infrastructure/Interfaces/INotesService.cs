using TelegramHelper.Domain.Entities;

namespace TelegramHelper.Infrastructure.Interfaces;

public interface INotesService
{
    Task AddNote(Note note, Guid categoryId);
    Task<Note> GetNoteById(Guid id);
    Task<List<Note>> GetByTitlePart(string name, int skip, int take);
    Task<List<Note>> GetNotesByCategoryId(Guid id);
}