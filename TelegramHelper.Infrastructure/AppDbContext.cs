using Microsoft.EntityFrameworkCore;
using TelegramHelper.Domain.Entities;

namespace TelegramHelper.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<Note> Notes { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<User> Users { get; set; }

    public AppDbContext(DbContextOptions options) : base(options)
    {
    }
}