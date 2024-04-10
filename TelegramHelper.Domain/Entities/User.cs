using TelegramHelper.Domain.Enums;

namespace TelegramHelper.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public long ChatId { get; set; }
    public UserRole Role { get; set; }
}