using TelegramHelper.Domain.Entities;
using TelegramHelper.Domain.Models;

namespace TelegramHelper.Infrastructure.Interfaces;

public interface ICategoriesService
{
    Task AddCategory(Category category);
    Task<Category?> GetCategoryById(Guid id, bool includeParent = false);
    Task<ReadResult<Category>> GetCategories(int skip, int take);
    Task<ReadResult<Category>> GetSubCategories(Guid categoryId, int skip, int take);
    Task Delete(Guid categoryId);
}