using TelegramHelper.Domain.Enums;

namespace TelegramHelper.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string Username { get; set; }
    public string LanguageCode { get; set; }
    public long ChatId { get; set; }
    public UserRole Role { get; set; }
}