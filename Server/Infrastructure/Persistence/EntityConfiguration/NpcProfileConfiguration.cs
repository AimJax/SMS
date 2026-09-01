using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Infrastructure.Persistence.EntityConfiguration;

public class NpcProfileConfiguration : IEntityTypeConfiguration<NpcProfile>
{
    public void Configure(EntityTypeBuilder<NpcProfile> builder)
    {
        builder.ToTable("NpcProfiles");

        builder.HasKey(e => e.Id);

        // Unique index on NpcId (stable identity)
        builder.HasIndex(e => e.NpcId).IsUnique();

        // Unique index on AccountId (one NPC per account)
        builder.HasIndex(e => e.AccountId).IsUnique();

        // Index for simulation scheduling
        builder.HasIndex(e => new { e.IsActive, e.NextSimulationAt });

        // Index for finding NPCs due for simulation
        builder.HasIndex(e => e.NextSimulationAt);

        // Required fields
        builder.Property(e => e.NpcId).IsRequired();
        builder.Property(e => e.AccountId).IsRequired();
        builder.Property(e => e.IsActive).IsRequired();
        builder.Property(e => e.ActivityState).IsRequired();
        builder.Property(e => e.SimulationIntervalSeconds).IsRequired();
        builder.Property(e => e.SimulationVersion).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();

        // Relationship with Account (one-to-one)
        builder.HasOne(e => e.Account)
            .WithOne(a => a.NpcProfile)
            .HasForeignKey<NpcProfile>(e => e.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with Personality (one-to-one)
        builder.HasOne(e => e.Personality)
            .WithOne(p => p.NpcProfile)
            .HasForeignKey<NpcPersonality>(p => p.NpcProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with Interests (one-to-many)
        builder.HasMany(e => e.Interests)
            .WithOne(i => i.NpcProfile)
            .HasForeignKey(i => i.NpcProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
