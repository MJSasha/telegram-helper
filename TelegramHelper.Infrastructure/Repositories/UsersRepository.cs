using Microsoft.EntityFrameworkCore;
using TelegramHelper.Domain.Entities;

namespace TelegramHelper.Infrastructure.Repositories;

internal class UsersRepository
{
    private readonly AppDbContext _dbContext;

    public UsersRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddUser(User user)
    {
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
    }

    public Task<User> GetUser(long chatId)
    {
        return _dbContext.Users.FirstOrDefaultAsync(x => x.ChatId == chatId);
    }

    public Task<User> GetUser(Guid id)
    {
        return _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
    }
}