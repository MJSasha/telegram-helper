using TelegramHelper.Domain.Entities;
using TelegramHelper.Domain.Models;

namespace TelegramHelper.Infrastructure.Interfaces;

public interface INotesService
{
    Task AddNote(Note note, Guid categoryId);
    Task<Note> GetNoteById(Guid id);
    Task<List<Note>> GetByTitlePart(string name, int skip, int take);
    Task<ReadResult<Note>> GetNotesByCategoryId(Guid id, int skip, int take);
}