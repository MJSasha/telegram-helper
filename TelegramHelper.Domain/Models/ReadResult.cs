namespace TelegramHelper.Domain.Models;

public class ReadResult<T>
{
    public List<T> Data { get; set; }
    public long TotalCount { get; set; }
}