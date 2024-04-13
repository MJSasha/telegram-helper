using TelegramHelper.Domain.Entities;
using TelegramHelper.Domain.Enums;
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

    public async Task<bool> CheckUserCanEdit(long chatId)
    {
        var currentUser = await GetUser(chatId);
        return currentUser?.Role is UserRole.Admin or UserRole.Editor;
    }

    public bool CheckUserCanEdit(User currentUser)
    {
        return currentUser?.Role is UserRole.Admin or UserRole.Editor;
    }

    public async Task<bool> CheckUserCanDelete(long chatId)
    {
        var currentUser = await GetUser(chatId);
        return currentUser?.Role is UserRole.Admin;
    }

    public bool CheckUserCanDelete(User currentUser)
    {
        return currentUser?.Role is UserRole.Admin;
    }
}