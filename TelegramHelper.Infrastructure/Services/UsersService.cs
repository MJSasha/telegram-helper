using TelegramHelper.Domain.Entities;
using TelegramHelper.Infrastructure.Repositories;

namespace TelegramHelper.Infrastructure.Services;

internal class UsersService : IUsersService
{
    private readonly UsersRepository _usersRepository;

    public UsersService(UsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }


    public Task AddUser(User user) => _usersRepository.AddUser(user);

    public Task<User> GetUser(long chatId) => _usersRepository.GetUser(chatId);

    public Task<User> GetUser(Guid id) => _usersRepository.GetUser(id);
}