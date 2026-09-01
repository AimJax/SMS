using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Infrastructure.Persistence.EntityConfiguration;

public class AccountHistoryConfiguration : IEntityTypeConfiguration<AccountHistory>
{
    public void Configure(EntityTypeBuilder<AccountHistory> builder)
    {
        builder.ToTable("AccountHistory");

        builder.HasKey(e => e.Id);

        // Index on AccountId for history queries
        builder.HasIndex(e => e.AccountId);

        // Index on CreatedAt for date-based queries
        builder.HasIndex(e => e.CreatedAt);

        // Required fields
        builder.Property(e => e.AccountId).IsRequired();
        builder.Property(e => e.EventType).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        // Optional fields
        builder.Property(e => e.Details).HasMaxLength(1000);
    }
}
