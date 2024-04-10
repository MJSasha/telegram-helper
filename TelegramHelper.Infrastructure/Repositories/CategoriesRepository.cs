using Microsoft.EntityFrameworkCore;
using TelegramHelper.Domain.Entities;
using TelegramHelper.Domain.Models;

namespace TelegramHelper.Infrastructure.Repositories;

internal class CategoriesRepository
{
    private readonly AppDbContext _dbContext;

    public CategoriesRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddCategory(Category category)
    {
        await _dbContext.Categories.AddAsync(category);
        await _dbContext.SaveChangesAsync();
    }

    public Task<Category> GetCategoryById(Guid id)
    {
        return _dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ReadResult<Category>> GetCategories(int skip, int take)
    {
        var result = await _dbContext.Categories.Where(c => c.ParentCategoryId == null).Skip(skip).Take(take).ToListAsync();
        var totalCount = await _dbContext.Categories.Where(c => c.ParentCategoryId == null).CountAsync();
        return new ReadResult<Category>
        {
            Data = result,
            TotalCount = totalCount
        };
    }

    public Task<List<Category>> GetSubCategories(Guid id, int skip, int take)
    {
        return _dbContext.Categories.Where(x => x.ParentCategoryId == id).Skip(skip).Take(take).ToListAsync();
    }
}