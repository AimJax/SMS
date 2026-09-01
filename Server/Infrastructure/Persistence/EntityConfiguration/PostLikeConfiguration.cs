using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Infrastructure.Persistence.EntityConfiguration;

public class PostLikeConfiguration : IEntityTypeConfiguration<PostLike>
{
    public void Configure(EntityTypeBuilder<PostLike> builder)
    {
        builder.ToTable("PostLikes");

        builder.HasKey(e => e.Id);

        // Unique constraint on like relationship to prevent duplicates
        builder.HasIndex(e => new { e.PostId, e.AccountId }).IsUnique();

        // Index for querying likes on a post
        builder.HasIndex(e => e.PostId);

        // Index for querying all likes by an account
        builder.HasIndex(e => e.AccountId);

        // Index for ordering by creation date
        builder.HasIndex(e => e.CreatedAt);

        // Required fields
        builder.Property(e => e.PostId).IsRequired();
        builder.Property(e => e.AccountId).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        // Relationship with Post
        builder.HasOne(e => e.Post)
            .WithMany(p => p.Likes)
            .HasForeignKey(e => e.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with Account
        builder.HasOne(e => e.Account)
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
