using Microsoft.EntityFrameworkCore;
using TelegramHelper.Domain.Entities;
using TelegramHelper.Domain.Models;

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

    public async Task<ReadResult<Note>> GetNotesByCategoryId(Guid id, int skip, int take)
    {
        var result = await _dbContext.Notes.Where(x => x.CategoryId == id).Skip(skip).Take(take).ToListAsync();
        var totalCount = await _dbContext.Notes.Where(x => x.CategoryId == id).CountAsync();
        return new ReadResult<Note>
        {
            Data = result,
            TotalCount = totalCount
        };
    }
}