using TelegramHelper.Domain.Entities;
using TelegramHelper.Infrastructure.Interfaces;
using TelegramHelper.Infrastructure.Repositories;

namespace TelegramHelper.Infrastructure.Services;

internal class CategoriesService : ICategoriesService
{
    private readonly CategoriesRepository _categoriesRepository;

    public CategoriesService(CategoriesRepository categoriesRepository)
    {
        _categoriesRepository = categoriesRepository;
    }

    public Task AddCategory(Category category) => _categoriesRepository.AddCategory(category);

    public Task<Category> GetCategoryById(Guid id) => _categoriesRepository.GetCategoryById(id);

    public Task<List<Category>> GetCategories(int skip, int take) => _categoriesRepository.GetCategories(skip, take);

    public Task<List<Category>> GetSubCategories(Guid categoryId, int skip, int take) => _categoriesRepository.GetSubCategories(categoryId, skip, take);
}