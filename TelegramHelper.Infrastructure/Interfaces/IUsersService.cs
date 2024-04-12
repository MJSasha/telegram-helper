using TelegramHelper.Domain.Entities;

namespace TelegramHelper.Infrastructure;

public interface IUsersService
{
    Task AddUser(User user);
    Task<User> GetUser(long chatId);
    Task<User> GetUser(Guid id);
}