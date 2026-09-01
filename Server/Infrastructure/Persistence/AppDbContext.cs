using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence.EntityConfiguration;

namespace SocialMediaSimulator.Server.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<AccountHistory> AccountHistory => Set<AccountHistory>();
    public DbSet<PersistenceTest> PersistenceTests => Set<PersistenceTest>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<Block> Blocks => Set<Block>();
    public DbSet<Mute> Mutes => Set<Mute>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostLike> PostLikes => Set<PostLike>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<NpcProfile> NpcProfiles => Set<NpcProfile>();
    public DbSet<NpcPersonality> NpcPersonalities => Set<NpcPersonality>();
    public DbSet<NpcInterest> NpcInterests => Set<NpcInterest>();
    public DbSet<NpcAction> NpcActions => Set<NpcAction>();
    public DbSet<AiProviderConfig> AiProviderConfigs => Set<AiProviderConfig>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Community> Communities => Set<Community>();
    public DbSet<CommunityMembership> CommunityMemberships => Set<CommunityMembership>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new ProfileConfiguration());
        modelBuilder.ApplyConfiguration(new AccountHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new PersistenceTestConfiguration());
        modelBuilder.ApplyConfiguration(new FollowConfiguration());
        modelBuilder.ApplyConfiguration(new BlockConfiguration());
        modelBuilder.ApplyConfiguration(new MuteConfiguration());
        modelBuilder.ApplyConfiguration(new PostConfiguration());
        modelBuilder.ApplyConfiguration(new PostLikeConfiguration());
        modelBuilder.ApplyConfiguration(new CommentConfiguration());
        modelBuilder.ApplyConfiguration(new NpcProfileConfiguration());
        modelBuilder.ApplyConfiguration(new NpcPersonalityConfiguration());
        modelBuilder.ApplyConfiguration(new NpcInterestConfiguration());
        modelBuilder.ApplyConfiguration(new NpcActionConfiguration());
        
        // Apply AI provider configuration
        modelBuilder.Entity<AiProviderConfig>(entity =>
        {
            entity.ToTable("AiProviderConfigs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Provider).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Model).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ApiKey).IsRequired().HasMaxLength(500);
            entity.Property(e => e.BaseUrl).HasMaxLength(500);
            entity.HasIndex(e => e.Provider).IsUnique();
        });
        
        // Apply Notification configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasKey(e => e.Id);
            
            // Indexes for efficient queries
            // Feed query: get notifications for recipient ordered by time
            entity.HasIndex(e => new { e.RecipientAccountId, e.CreatedAt })
                .IsDescending(false, true);
            
            // Unread count: count unread notifications for recipient
            entity.HasIndex(e => new { e.RecipientAccountId, e.IsRead });
            
            // Navigation property configurations
            entity.HasOne(e => e.RecipientAccount)
                .WithMany()
                .HasForeignKey(e => e.RecipientAccountId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.ActorAccount)
                .WithMany()
                .HasForeignKey(e => e.ActorAccountId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.RelatedPost)
                .WithMany()
                .HasForeignKey(e => e.RelatedPostId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // Apply Community configuration
        modelBuilder.Entity<Community>(entity =>
        {
            entity.ToTable("Communities");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Topic).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Tags).HasMaxLength(500);
            
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.Topic);
            entity.HasIndex(e => e.MemberCount);
            entity.HasIndex(e => e.IsActive);
            
            entity.HasOne(e => e.OwnerAccount)
                .WithMany()
                .HasForeignKey(e => e.OwnerAccountId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasMany(e => e.Memberships)
                .WithOne(m => m.Community)
                .HasForeignKey(m => m.CommunityId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Posts)
                .WithOne(p => p.Community)
                .HasForeignKey(p => p.CommunityId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // Apply CommunityMembership configuration
        modelBuilder.Entity<CommunityMembership>(entity =>
        {
            entity.ToTable("CommunityMemberships");
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => new { e.CommunityId, e.AccountId }).IsUnique();
            entity.HasIndex(e => new { e.CommunityId, e.IsActive });
            entity.HasIndex(e => new { e.AccountId, e.IsActive });
            
            entity.HasOne(e => e.Community)
                .WithMany(c => c.Memberships)
                .HasForeignKey(e => e.CommunityId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Update Post entity configuration to include CommunityId
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasOne(p => p.Community)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.CommunityId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
