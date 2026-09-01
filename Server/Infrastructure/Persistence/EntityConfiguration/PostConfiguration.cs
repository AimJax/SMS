using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Infrastructure.Persistence.EntityConfiguration;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Posts");

        builder.HasKey(e => e.Id);

        // Unique index on PostId (stable identity)
        builder.HasIndex(e => e.PostId).IsUnique();

        // Index for querying by author
        builder.HasIndex(e => e.AuthorAccountId);

        // Index for ordering by creation date
        builder.HasIndex(e => e.CreatedAt);

        // Composite index for author + created date (common query pattern)
        builder.HasIndex(e => new { e.AuthorAccountId, e.CreatedAt });

        // Index for status (for soft delete filtering)
        builder.HasIndex(e => e.Status);

        // Required fields
        builder.Property(e => e.PostId).IsRequired();
        builder.Property(e => e.AuthorAccountId).IsRequired();
        builder.Property(e => e.Content).IsRequired().HasMaxLength(10000);
        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();

        // Relationship with Account
        builder.HasOne(e => e.AuthorAccount)
            .WithMany()
            .HasForeignKey(e => e.AuthorAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with Likes
        builder.HasMany(e => e.Likes)
            .WithOne(l => l.Post)
            .HasForeignKey(l => l.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with Comments
        builder.HasMany(e => e.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
