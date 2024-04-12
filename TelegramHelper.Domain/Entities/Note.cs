namespace TelegramHelper.Domain.Entities;

public class Note
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public Guid CategoryId { get; set; }
    public Category Category { get; set; }
    public Guid AuthorId { get; set; }
    public User Author { get; set; }
}