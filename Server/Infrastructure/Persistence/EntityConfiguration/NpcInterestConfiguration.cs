using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Infrastructure.Persistence.EntityConfiguration;

public class NpcInterestConfiguration : IEntityTypeConfiguration<NpcInterest>
{
    public void Configure(EntityTypeBuilder<NpcInterest> builder)
    {
        builder.ToTable("NpcInterests");

        builder.HasKey(e => e.Id);

        // Composite unique index on (NpcProfileId, InterestKey)
        builder.HasIndex(e => new { e.NpcProfileId, e.InterestKey }).IsUnique();

        // Index for querying interests by category
        builder.HasIndex(e => e.InterestKey);

        // Required fields
        builder.Property(e => e.InterestKey).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Strength).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
    }
}
