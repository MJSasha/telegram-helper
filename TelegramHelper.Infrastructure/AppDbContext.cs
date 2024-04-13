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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>()
            .HasMany(c => c.Notes)
            .WithOne(n => n.Category)
            .HasForeignKey(n => n.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}