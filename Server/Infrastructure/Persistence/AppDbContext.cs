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
    public DbSet<CausalChain> CausalChains => Set<CausalChain>();
    public DbSet<OfflineSimulationResult> OfflineSimulationResults => Set<OfflineSimulationResult>();
    public DbSet<PostVirality> PostVirality => Set<PostVirality>();
    public DbSet<ViralityTransition> ViralityTransitions => Set<ViralityTransition>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<Hashtag> Hashtags => Set<Hashtag>();
    public DbSet<Trend> Trends => Set<Trend>();
    public DbSet<TrendPropagation> TrendPropagations => Set<TrendPropagation>();
    public DbSet<TopicSubscription> TopicSubscriptions => Set<TopicSubscription>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<Rumor> Rumors => Set<Rumor>();
    public DbSet<AccountBelief> AccountBeliefs => Set<AccountBelief>();
    public DbSet<RumorEvidence> RumorEvidence => Set<RumorEvidence>();
    public DbSet<NewsAccount> NewsAccounts => Set<NewsAccount>();
    public DbSet<NewsArticle> NewsArticles => Set<NewsArticle>();
    public DbSet<NewsExposure> NewsExposures => Set<NewsExposure>();

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
        
        // Apply CausalChain configuration
        modelBuilder.Entity<CausalChain>(entity =>
        {
            entity.ToTable("CausalChain");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.CauseDescription).IsRequired();
            entity.Property(e => e.Metadata).HasColumnType("TEXT");
            
            // Indexes for efficient queries
            entity.HasIndex(e => e.EventId);
            entity.HasIndex(e => e.CauseEventId);
            entity.HasIndex(e => e.AccountId);
            
            // Use EventId (Guid) to reference Event.EventId (Guid), not Event.Id (int)
            entity.HasOne(e => e.Event)
                .WithMany()
                .HasForeignKey(e => e.EventId)
                .HasPrincipalKey(ev => ev.EventId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.CauseEvent)
                .WithMany()
                .HasForeignKey(e => e.CauseEventId)
                .HasPrincipalKey(ev => ev.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Apply OfflineSimulationResult configuration
        modelBuilder.Entity<OfflineSimulationResult>(entity =>
        {
            entity.ToTable("OfflineSimulationResults");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Duration).HasConversion<string>();
            entity.Property(e => e.EventsSummaryJson).HasColumnType("TEXT");
            entity.Property(e => e.CatchupSummary).IsRequired();
            
            // Indexes for efficient queries
            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.AccountId, e.IsAcknowledged });
            
            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Add Event indexes for parent-child relationships
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasIndex(e => e.ParentEventId);
            entity.HasIndex(e => e.TriggerEventId);
            entity.HasIndex(e => e.EventChainId);
        });
        
        // PostVirality entity configuration
        modelBuilder.Entity<PostVirality>(entity =>
        {
            entity.ToTable("PostVirality");
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.PostViralityId).IsRequired();
            entity.Property(e => e.PostId).IsRequired();
            entity.Property(e => e.State).IsRequired();
            entity.Property(e => e.Score).IsRequired();
            entity.Property(e => e.TotalEngagement).IsRequired();
            entity.Property(e => e.Velocity).IsRequired();
            entity.Property(e => e.PeakVelocity).IsRequired();
            entity.Property(e => e.Reach).IsRequired();
            entity.Property(e => e.ShareCount).IsRequired();
            entity.Property(e => e.ControversyLevel).IsRequired();
            entity.Property(e => e.HasControversyAnalysis).IsRequired();
            entity.Property(e => e.LastUpdated).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            // Indexes
            entity.HasIndex(e => e.PostId).IsUnique();
            entity.HasIndex(e => e.State);
            entity.HasIndex(e => e.Score);
            entity.HasIndex(e => e.Velocity);
            entity.HasIndex(e => e.LastUpdated);
            
            // Use Guid PostId to reference Post.PostId (not Post.Id)
            entity.HasOne(e => e.Post)
                .WithMany()
                .HasForeignKey(e => e.PostId)
                .HasPrincipalKey(p => p.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // ViralityTransition entity configuration
        modelBuilder.Entity<ViralityTransition>(entity =>
        {
            entity.ToTable("ViralityTransition");
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.TransitionId).IsRequired();
            entity.Property(e => e.PostId).IsRequired();
            entity.Property(e => e.FromState).IsRequired();
            entity.Property(e => e.ToState).IsRequired();
            entity.Property(e => e.ScoreAtTransition).IsRequired();
            entity.Property(e => e.EngagementAtTransition).IsRequired();
            entity.Property(e => e.VelocityAtTransition).IsRequired();
            entity.Property(e => e.TransitionedAt).IsRequired();
            
            // Indexes
            entity.HasIndex(e => e.PostId);
            entity.HasIndex(e => e.TransitionedAt);
            entity.HasIndex(e => new { e.PostId, e.TransitionedAt });
            
            // Use Guid PostId to reference Post.PostId (not Post.Id)
            entity.HasOne(e => e.Post)
                .WithMany()
                .HasForeignKey(e => e.PostId)
                .HasPrincipalKey(p => p.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Topic entity configuration
        modelBuilder.Entity<Topic>(entity =>
        {
            entity.ToTable("Topics");
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.TopicId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Category).IsRequired();
            entity.Property(e => e.IsVerified).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsActive);
        });
        
        // Hashtag entity configuration
        modelBuilder.Entity<Hashtag>(entity =>
        {
            entity.ToTable("Hashtags");
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.HashtagId).IsRequired();
            entity.Property(e => e.Tag).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NormalizedTag).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IsTrending).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            entity.HasIndex(e => e.Tag).IsUnique();
            entity.HasIndex(e => e.NormalizedTag).IsUnique();
            entity.HasIndex(e => e.IsTrending);
            entity.HasIndex(e => e.TodayUsageCount);
            
            // Use Guid TopicId to reference Topic.TopicId (not Topic.Id)
            entity.HasOne(e => e.Topic)
                .WithMany(t => t.Hashtags)
                .HasForeignKey(e => e.TopicId)
                .HasPrincipalKey(t => t.TopicId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // Trend entity configuration
        modelBuilder.Entity<Trend>(entity =>
        {
            entity.ToTable("Trends");
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.TrendId).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Query).IsRequired().HasMaxLength(500);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Strength).IsRequired();
            entity.Property(e => e.Scope).IsRequired();
            entity.Property(e => e.CalculatedAt).IsRequired();
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasIndex(e => e.Query);
            entity.HasIndex(e => e.Scope);
            entity.HasIndex(e => e.Strength);
            entity.HasIndex(e => e.Rank);
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => e.IsActive);
            
            // Use Guid TopicId to reference Topic.TopicId (not Topic.Id)
            entity.HasOne(e => e.Topic)
                .WithMany()
                .HasForeignKey(e => e.TopicId)
                .HasPrincipalKey(t => t.TopicId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Use Guid HashtagId to reference Hashtag.HashtagId (not Hashtag.Id)
            entity.HasOne(e => e.Hashtag)
                .WithMany()
                .HasForeignKey(e => e.HashtagId)
                .HasPrincipalKey(h => h.HashtagId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Community)
                .WithMany()
                .HasForeignKey(e => e.CommunityId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // TrendPropagation entity configuration
        modelBuilder.Entity<TrendPropagation>(entity =>
        {
            entity.ToTable("TrendPropagations");
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.PropagationId).IsRequired();
            entity.Property(e => e.TrendId).IsRequired();
            entity.Property(e => e.FromCommunityId).IsRequired();
            entity.Property(e => e.ToCommunityId).IsRequired();
            entity.Property(e => e.PropagatedAt).IsRequired();
            
            // Use Guid TrendId to reference Trend.TrendId (not Trend.Id)
            entity.HasOne(e => e.Trend)
                .WithMany()
                .HasForeignKey(e => e.TrendId)
                .HasPrincipalKey(t => t.TrendId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.TrendId);
        });
        
        // TopicSubscription entity configuration
        modelBuilder.Entity<TopicSubscription>(entity =>
        {
            entity.ToTable("TopicSubscriptions");
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.SubscribedAt).IsRequired();
            
            entity.HasIndex(e => new { e.AccountId, e.TopicId }).IsUnique();
        });
        
        // AppSetting entity configuration
        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.ToTable("AppSettings");
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Key).IsRequired().HasMaxLength(200);
            
            entity.HasIndex(e => e.Key).IsUnique();
        });
        
        // Rumor entity configuration
        modelBuilder.Entity<Rumor>(entity =>
        {
            entity.ToTable("Rumors");
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.RumorId).IsRequired();
            entity.Property(e => e.Claim).IsRequired();
            entity.Property(e => e.Summary).IsRequired();
            entity.Property(e => e.TruthStatus).IsRequired();
            entity.Property(e => e.SpreadType).IsRequired();
            entity.Property(e => e.FirstSeenAt).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasIndex(e => e.RumorId).IsUnique();
            entity.HasIndex(e => e.TruthStatus);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.CommunityId);
            entity.HasIndex(e => e.FirstSeenAt);
            
            // Use Guid SourcePostId to reference Post.PostId
            entity.HasOne(e => e.SourcePost)
                .WithMany()
                .HasForeignKey(e => e.SourcePostId)
                .HasPrincipalKey(p => p.PostId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Community)
                .WithMany()
                .HasForeignKey(e => e.CommunityId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // AccountBelief entity configuration
        modelBuilder.Entity<AccountBelief>(entity =>
        {
            entity.ToTable("AccountBeliefs");
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.AccountBeliefId).IsRequired();
            entity.Property(e => e.AccountId).IsRequired();
            entity.Property(e => e.RumorId).IsRequired();
            entity.Property(e => e.Belief).IsRequired();
            entity.Property(e => e.Confidence).IsRequired();
            entity.Property(e => e.FormedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            entity.HasIndex(e => e.AccountBeliefId).IsUnique();
            entity.HasIndex(e => new { e.AccountId, e.RumorId }).IsUnique();
            
            // Use Guid RumorId to reference Rumor.RumorId
            entity.HasOne(e => e.Rumor)
                .WithMany(r => r.Beliefs)
                .HasForeignKey(e => e.RumorId)
                .HasPrincipalKey(r => r.RumorId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // RumorEvidence entity configuration
        modelBuilder.Entity<RumorEvidence>(entity =>
        {
            entity.ToTable("RumorEvidence");
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.EvidenceId).IsRequired();
            entity.Property(e => e.RumorId).IsRequired();
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.SupportsRumor).IsRequired();
            entity.Property(e => e.CredibilityScore).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasIndex(e => e.EvidenceId).IsUnique();
            entity.HasIndex(e => e.RumorId);
            
            // Use Guid RumorId to reference Rumor.RumorId
            entity.HasOne(e => e.Rumor)
                .WithMany(r => r.Evidence)
                .HasForeignKey(e => e.RumorId)
                .HasPrincipalKey(r => r.RumorId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // NewsAccount entity configuration
        modelBuilder.Entity<NewsAccount>(entity =>
        {
            entity.ToTable("NewsAccounts");
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.NewsAccountId).IsRequired();
            entity.Property(e => e.AccountId).IsRequired();
            entity.Property(e => e.NewsName).IsRequired();
            entity.Property(e => e.Category).IsRequired();
            entity.Property(e => e.Tone).IsRequired();
            entity.Property(e => e.CredibilityScore).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            entity.HasIndex(e => e.NewsAccountId).IsUnique();
            entity.HasIndex(e => e.AccountId).IsUnique();
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsActive);
        });
        
        // NewsArticle entity configuration
        modelBuilder.Entity<NewsArticle>(entity =>
        {
            entity.ToTable("NewsArticles");
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.ArticleId).IsRequired();
            entity.Property(e => e.NewsAccountId).IsRequired();
            entity.Property(e => e.Headline).IsRequired();
            entity.Property(e => e.Category).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            entity.HasIndex(e => e.ArticleId).IsUnique();
            entity.HasIndex(e => e.NewsAccountId);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.PublishedAt);
            entity.HasIndex(e => e.IsBreakingNews);
            entity.HasIndex(e => e.Status);
            
            // Use Guid NewsAccountId to reference NewsAccount.NewsAccountId
            entity.HasOne(e => e.NewsAccount)
                .WithMany(n => n.Articles)
                .HasForeignKey(e => e.NewsAccountId)
                .HasPrincipalKey(n => n.NewsAccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // NewsExposure entity configuration
        modelBuilder.Entity<NewsExposure>(entity =>
        {
            entity.ToTable("NewsExposures");
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.ExposureId).IsRequired();
            entity.Property(e => e.ArticleId).IsRequired();
            entity.Property(e => e.CommunityId).IsRequired();
            entity.Property(e => e.ExposedAt).IsRequired();
            
            entity.HasIndex(e => e.ExposureId).IsUnique();
            entity.HasIndex(e => e.ArticleId);
            entity.HasIndex(e => e.CommunityId);
            
            // Use Guid ArticleId to reference NewsArticle.ArticleId
            entity.HasOne(e => e.Article)
                .WithMany(a => a.Exposures)
                .HasForeignKey(e => e.ArticleId)
                .HasPrincipalKey(a => a.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
