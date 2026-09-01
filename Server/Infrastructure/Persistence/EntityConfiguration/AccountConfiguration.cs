using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Infrastructure.Persistence.EntityConfiguration;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(e => e.Id);

        // Unique index on AccountId (stable identity)
        builder.HasIndex(e => e.AccountId).IsUnique();

        // Unique index on normalized username
        builder.HasIndex(e => e.UsernameNormalized).IsUnique();

        // Required fields
        builder.Property(e => e.Username).IsRequired().HasMaxLength(50);
        builder.Property(e => e.UsernameNormalized).IsRequired().HasMaxLength(50);
        builder.Property(e => e.PasswordHash).IsRequired();
        builder.Property(e => e.AccountId).IsRequired();
        builder.Property(e => e.AccountType).IsRequired();
        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();

        // Optional fields
        builder.Property(e => e.Email).HasMaxLength(255);

        // One-to-one relationship with Profile
        builder.HasOne(e => e.Profile)
            .WithOne(p => p.Account)
            .HasForeignKey<Profile>(p => p.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many relationship with AccountHistory
        builder.HasMany(e => e.History)
            .WithOne(h => h.Account)
            .HasForeignKey(h => h.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
