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

    public async Task<Category?> GetCategoryById(Guid id, bool includeParent)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id);
        await LoadParentCategoriesRecursive(category);
        return category;
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

    public async Task<ReadResult<Category>> GetSubCategories(Guid id, int skip, int take)
    {
        var result = await _dbContext.Categories.Where(x => x.ParentCategoryId == id).Skip(skip).Take(take).ToListAsync();
        var totalCount = await _dbContext.Categories.Where(x => x.ParentCategoryId == id).CountAsync();
        return new ReadResult<Category>
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
}