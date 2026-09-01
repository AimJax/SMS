using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Infrastructure.Persistence.EntityConfiguration;

public class NpcActionConfiguration : IEntityTypeConfiguration<NpcAction>
{
    public void Configure(EntityTypeBuilder<NpcAction> builder)
    {
        builder.ToTable("NpcActions");

        builder.HasKey(e => e.Id);

        // Index for finding actions by NPC
        builder.HasIndex(e => e.NpcProfileId);

        // Index for finding unexecuted actions
        builder.HasIndex(e => new { e.Executed, e.ScheduledAt });

        // Index for target lookups
        builder.HasIndex(e => e.TargetPostId);
        builder.HasIndex(e => e.TargetAccountId);

        // Optional fields with max length
        builder.Property(e => e.TargetPostId).HasMaxLength(50);
        builder.Property(e => e.TargetAccountId).HasMaxLength(50);
        builder.Property(e => e.Content).HasMaxLength(10000);

        // Required fields
        builder.Property(e => e.ActionType).IsRequired();
        builder.Property(e => e.Executed).IsRequired();
        builder.Property(e => e.ScheduledAt).IsRequired();

        // Relationship with NpcProfile
        builder.HasOne(e => e.NpcProfile)
            .WithMany()
            .HasForeignKey(e => e.NpcProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
