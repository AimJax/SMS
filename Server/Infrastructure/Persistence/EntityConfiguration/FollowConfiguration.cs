using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Infrastructure.Persistence.EntityConfiguration;

public class FollowConfiguration : IEntityTypeConfiguration<Follow>
{
    public void Configure(EntityTypeBuilder<Follow> builder)
    {
        builder.ToTable("Follows");

        builder.HasKey(e => e.Id);

        // Unique constraint on follow relationship to prevent duplicates
        builder.HasIndex(e => new { e.FollowerAccountId, e.FollowedAccountId }).IsUnique();

        // Index for querying followers of an account
        builder.HasIndex(e => e.FollowedAccountId);

        // Index for querying who an account follows
        builder.HasIndex(e => e.FollowerAccountId);

        // Index for ordering by creation date (pagination)
        builder.HasIndex(e => e.CreatedAt);

        // Required fields
        builder.Property(e => e.FollowerAccountId).IsRequired();
        builder.Property(e => e.FollowedAccountId).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        // Relationships
        builder.HasOne(e => e.FollowerAccount)
            .WithMany()
            .HasForeignKey(e => e.FollowerAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.FollowedAccount)
            .WithMany()
            .HasForeignKey(e => e.FollowedAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
