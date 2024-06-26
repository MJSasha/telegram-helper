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

    public async Task<Note> GetById(Guid id, bool includeCategories)
    {
        var query = _dbContext.Notes.Include(x => x.Category).AsQueryable();

        if (includeCategories)
        {
            var note = await query.FirstOrDefaultAsync(x => x.Id == id);
            if (note != null)
            {
                await LoadParentCategoriesRecursive(note.Category);
            }

            return note;
        }

        return await query.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Note>> GetByTitlePart(string name, int skip, int take, bool includeCategories)
    {
        var query = _dbContext.Notes
            .Where(x => x.Title.Contains(name))
            .Skip(skip)
            .Take(take)
            .Include(n => n.Category);

        if (includeCategories)
        {
            var notes = await query.ToListAsync();
            foreach (var note in notes)
            {
                await LoadParentCategoriesRecursive(note.Category);
            }
        }

        return await query.ToListAsync();
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

    private async Task LoadParentCategoriesRecursive(Category category)
    {
        if (category == null || category.ParentCategoryId == null)
            return;

        await _dbContext.Entry(category)
            .Reference(c => c.ParentCategory)
            .LoadAsync();

        await LoadParentCategoriesRecursive(category.ParentCategory);
    }

    public async Task Delete(Guid noteId)
    {
        var noteToDelete = await _dbContext.Notes.FirstOrDefaultAsync(n => n.Id == noteId);
        _dbContext.Notes.Remove(noteToDelete);
        await _dbContext.SaveChangesAsync();
    }
}