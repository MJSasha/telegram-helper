using Microsoft.EntityFrameworkCore;
using TelegramHelper.Domain.Entities;

namespace TelegramHelper.Infrastructure.Repositories;

internal class NotesRepository
{
    private readonly AppDbContext _dbContext;

    public NotesRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Create(Note note)
    {
        await _dbContext.Notes.AddAsync(note);
        await _dbContext.SaveChangesAsync();
    }

    public Task<Note> GetById(Guid id)
    {
        return _dbContext.Notes.FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<List<Note>> GetByTitlePart(string name, int skip, int take)
    {
        return _dbContext.Notes.Where(x => x.Title.Contains(name)).Skip(skip).Take(take).ToListAsync();
    }

    public Task<List<Note>> GetNotesByCategoryId(Guid id)
    {
        return _dbContext.Notes.Where(x => x.CategoryId == id).ToListAsync();
    }
}