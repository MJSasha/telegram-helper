using TelegramHelper.Domain.Entities;

namespace TelegramHelper.Infrastructure.Interfaces;

public interface ICategoriesService
{
    Task AddCategory(Category category);
    Task<Category> GetCategoryById(Guid id);
    Task<List<Category>> GetCategories(int skip, int take);
    Task<List<Category>> GetSubCategories(Guid categoryId, int skip, int take);
}