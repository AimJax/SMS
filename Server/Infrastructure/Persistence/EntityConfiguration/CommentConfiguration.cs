using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Infrastructure.Persistence.EntityConfiguration;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");

        builder.HasKey(e => e.Id);

        // Unique index on CommentId
        builder.HasIndex(e => e.CommentId).IsUnique();

        // Index for querying comments on a post
        builder.HasIndex(e => e.PostId);

        // Index for querying comments by author
        builder.HasIndex(e => e.AuthorAccountId);

        // Composite index for post + created date (common query pattern)
        builder.HasIndex(e => new { e.PostId, e.CreatedAt });

        // Index for status (for soft delete filtering)
        builder.HasIndex(e => e.Status);

        // Required fields
        builder.Property(e => e.CommentId).IsRequired();
        builder.Property(e => e.PostId).IsRequired();
        builder.Property(e => e.AuthorAccountId).IsRequired();
        builder.Property(e => e.Content).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();

        // Relationship with Post
        builder.HasOne(e => e.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(e => e.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with Account
        builder.HasOne(e => e.AuthorAccount)
            .WithMany()
            .HasForeignKey(e => e.AuthorAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
