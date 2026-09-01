using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Infrastructure.Persistence.EntityConfiguration;

public class MuteConfiguration : IEntityTypeConfiguration<Mute>
{
    public void Configure(EntityTypeBuilder<Mute> builder)
    {
        builder.ToTable("Mutes");

        builder.HasKey(e => e.Id);

        // Unique constraint on mute relationship to prevent duplicates
        builder.HasIndex(e => new { e.MuterAccountId, e.MutedAccountId }).IsUnique();

        // Index for querying who an account has muted
        builder.HasIndex(e => e.MuterAccountId);

        // Index for querying who muted an account
        builder.HasIndex(e => e.MutedAccountId);

        // Index for ordering by creation date
        builder.HasIndex(e => e.CreatedAt);

        // Required fields
        builder.Property(e => e.MuterAccountId).IsRequired();
        builder.Property(e => e.MutedAccountId).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        // Relationships
        builder.HasOne(e => e.MuterAccount)
            .WithMany()
            .HasForeignKey(e => e.MuterAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.MutedAccount)
            .WithMany()
            .HasForeignKey(e => e.MutedAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
