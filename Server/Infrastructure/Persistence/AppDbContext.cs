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
    }
}
