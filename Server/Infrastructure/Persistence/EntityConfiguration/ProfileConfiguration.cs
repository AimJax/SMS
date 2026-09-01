using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Infrastructure.Persistence.EntityConfiguration;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("Profiles");

        builder.HasKey(e => e.Id);

        // Index on AccountId (unique as it's one-to-one)
        builder.HasIndex(e => e.AccountId).IsUnique();

        // Required fields
        builder.Property(e => e.AccountId).IsRequired();
        builder.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();

        // Optional fields
        builder.Property(e => e.Bio).HasMaxLength(500);
        builder.Property(e => e.AvatarUrl).HasMaxLength(500);
    }
}
