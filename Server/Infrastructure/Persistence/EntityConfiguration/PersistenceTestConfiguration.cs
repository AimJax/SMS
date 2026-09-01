using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Infrastructure.Persistence.EntityConfiguration;

public class PersistenceTestConfiguration : IEntityTypeConfiguration<PersistenceTest>
{
    public void Configure(EntityTypeBuilder<PersistenceTest> builder)
    {
        builder.ToTable("PersistenceTests");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Value)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // Index for date-based queries
        builder.HasIndex(e => e.CreatedAt);
    }
}
