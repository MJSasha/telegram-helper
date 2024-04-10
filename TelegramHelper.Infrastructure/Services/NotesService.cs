using TelegramHelper.Domain.Entities;
using TelegramHelper.Infrastructure.Interfaces;
using TelegramHelper.Infrastructure.Repositories;

namespace TelegramHelper.Infrastructure.Services;

internal class NotesService : INotesService
{
    private readonly NotesRepository _notesRepository;

    public NotesService(NotesRepository notesRepository)
    {
        _notesRepository = notesRepository;
    }

    public async Task AddNote(Note note, Guid categoryId)
    {
        note.CategoryId = categoryId;

        // TODO: check note exist
        await _notesRepository.Create(note);
    }

    public Task<Note> GetNoteById(Guid id) => _notesRepository.GetById(id);

    public Task<List<Note>> GetByTitlePart(string name, int skip, int take) => _notesRepository.GetByTitlePart(name, skip, take);

    public Task<List<Note>> GetNotesByCategoryId(Guid id) => _notesRepository.GetNotesByCategoryId(id);
}