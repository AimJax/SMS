using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Infrastructure.Persistence.EntityConfiguration;

public class BlockConfiguration : IEntityTypeConfiguration<Block>
{
    public void Configure(EntityTypeBuilder<Block> builder)
    {
        builder.ToTable("Blocks");

        builder.HasKey(e => e.Id);

        // Unique constraint on block relationship to prevent duplicates
        builder.HasIndex(e => new { e.BlockerAccountId, e.BlockedAccountId }).IsUnique();

        // Index for querying who an account has blocked
        builder.HasIndex(e => e.BlockerAccountId);

        // Index for querying who blocked an account
        builder.HasIndex(e => e.BlockedAccountId);

        // Index for ordering by creation date
        builder.HasIndex(e => e.CreatedAt);

        // Required fields
        builder.Property(e => e.BlockerAccountId).IsRequired();
        builder.Property(e => e.BlockedAccountId).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        // Relationships
        builder.HasOne(e => e.BlockerAccount)
            .WithMany()
            .HasForeignKey(e => e.BlockerAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.BlockedAccount)
            .WithMany()
            .HasForeignKey(e => e.BlockedAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
