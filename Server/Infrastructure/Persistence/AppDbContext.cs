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
    public DbSet<FeedImpression> FeedImpressions => Set<FeedImpression>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventParticipation> EventParticipations => Set<EventParticipation>();
    public DbSet<EventConsequence> EventConsequences => Set<EventConsequence>();

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
        
        // Update Post entity configuration to include CommunityId and Topic
        modelBuilder.Entity<Post>(entity =>
        {
            entity.Property(p => p.Topic).HasMaxLength(100);
            entity.HasIndex(p => p.Topic);
            entity.HasIndex(p => p.CreatedAt);
            entity.HasIndex(p => p.AuthorAccountId);
            
            entity.HasOne(p => p.Community)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.CommunityId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // Apply FeedImpression configuration
        modelBuilder.Entity<FeedImpression>(entity =>
        {
            entity.ToTable("FeedImpressions");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Score).IsRequired();
            entity.Property(e => e.Position).IsRequired();
            
            // Indexes for efficient queries
            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => e.PostId);
            entity.HasIndex(e => new { e.AccountId, e.CreatedAt });
            entity.HasIndex(e => new { e.AccountId, e.PostId });
            
            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Post)
                .WithMany()
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Apply Event configuration
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Events");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.NarrativeContext).IsRequired();
            entity.Property(e => e.Topic).HasMaxLength(100);
            entity.Property(e => e.Metadata).HasColumnType("TEXT");
            
            // Indexes for efficient queries
            entity.HasIndex(e => new { e.Status, e.CreatedAt });
            entity.HasIndex(e => new { e.Type, e.Status });
            entity.HasIndex(e => e.Topic);
            entity.HasIndex(e => e.CommunityId);
            entity.HasIndex(e => e.IsDeleted);
            
            entity.HasOne(e => e.CreatorAccount)
                .WithMany()
                .HasForeignKey(e => e.CreatorAccountId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Community)
                .WithMany()
                .HasForeignKey(e => e.CommunityId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasMany(e => e.Participations)
                .WithOne(p => p.Event)
                .HasForeignKey(p => p.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Apply EventParticipation configuration
        modelBuilder.Entity<EventParticipation>(entity =>
        {
            entity.ToTable("EventParticipations");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.LLMReasoning).IsRequired();
            
            // Indexes for efficient queries
            entity.HasIndex(e => new { e.EventId, e.AccountId });
            entity.HasIndex(e => e.AccountId);
            
            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Apply EventConsequence configuration
        modelBuilder.Entity<EventConsequence>(entity =>
        {
            entity.ToTable("EventConsequences");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Parameters).HasColumnType("TEXT");
            entity.Property(e => e.FailureReason).HasMaxLength(500);
            
            // Index for audit queries
            entity.HasIndex(e => e.EventId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.ProcessedAt);
            
            entity.HasOne(e => e.Event)
                .WithMany()
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
