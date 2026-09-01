using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Infrastructure.Persistence.EntityConfiguration;

public class NpcPersonalityConfiguration : IEntityTypeConfiguration<NpcPersonality>
{
    public void Configure(EntityTypeBuilder<NpcPersonality> builder)
    {
        builder.ToTable("NpcPersonalities");

        builder.HasKey(e => e.Id);

        // Index for NPC lookup
        builder.HasIndex(e => e.NpcProfileId).IsUnique();

        // Personality traits (0.0 - 1.0)
        builder.Property(e => e.Openness).IsRequired();
        builder.Property(e => e.Conscientiousness).IsRequired();
        builder.Property(e => e.Extraversion).IsRequired();
        builder.Property(e => e.Agreeableness).IsRequired();
        builder.Property(e => e.Neuroticism).IsRequired();

        builder.Property(e => e.GeneratedAt).IsRequired();
    }
}
