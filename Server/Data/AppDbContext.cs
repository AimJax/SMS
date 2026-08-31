using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Data.Entities;

namespace SocialMediaSimulator.Server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<PersistenceTest> PersistenceTests { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PersistenceTest>(entity =>
        {
            entity.ToTable("PersistenceTests");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }
}
