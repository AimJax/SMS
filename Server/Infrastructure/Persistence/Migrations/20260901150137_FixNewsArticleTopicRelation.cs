using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaSimulator.Server.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixNewsArticleTopicRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "FameLevel",
                table: "Profiles",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "FollowerCount",
                table: "Profiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CommunityId",
                table: "Posts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Topic",
                table: "Posts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenAt",
                table: "Accounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Posts_PostId",
                table: "Posts",
                column: "PostId");

            migrationBuilder.CreateTable(
                name: "AiProviderConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ApiKey = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    BaseUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiProviderConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    ValueDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeedImpressions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    PostId = table.Column<int>(type: "INTEGER", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    Clicked = table.Column<bool>(type: "INTEGER", nullable: false),
                    Liked = table.Column<bool>(type: "INTEGER", nullable: false),
                    Commented = table.Column<bool>(type: "INTEGER", nullable: false),
                    Shared = table.Column<bool>(type: "INTEGER", nullable: false),
                    Skipped = table.Column<bool>(type: "INTEGER", nullable: false),
                    Score = table.Column<double>(type: "REAL", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedImpressions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedImpressions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeedImpressions_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NewsAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NewsAccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    NewsName = table.Column<string>(type: "TEXT", nullable: false),
                    NewsTagline = table.Column<string>(type: "TEXT", nullable: false),
                    NewsBio = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    CredibilityScore = table.Column<int>(type: "INTEGER", nullable: false),
                    SubscriberCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ArticlesPublished = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalArticleViews = table.Column<int>(type: "INTEGER", nullable: false),
                    AccuracyRating = table.Column<double>(type: "REAL", nullable: false),
                    BreakingNewsCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Tone = table.Column<int>(type: "INTEGER", nullable: false),
                    ReportFrequency = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsAccounts", x => x.Id);
                    table.UniqueConstraint("AK_NewsAccounts_NewsAccountId", x => x.NewsAccountId);
                    table.ForeignKey(
                        name: "FK_NewsAccounts_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecipientAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    ActorAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    RelatedEntityId = table.Column<int>(type: "INTEGER", nullable: false),
                    RelatedPostGuid = table.Column<Guid>(type: "TEXT", nullable: true),
                    RelatedPostId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Accounts_ActorAccountId",
                        column: x => x.ActorAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Accounts_RecipientAccountId",
                        column: x => x.RecipientAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Posts_RelatedPostId",
                        column: x => x.RelatedPostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OfflineSimulationResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OfflineSimulationResultId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Duration = table.Column<string>(type: "TEXT", nullable: false),
                    TicksSimulated = table.Column<int>(type: "INTEGER", nullable: false),
                    PostsCreated = table.Column<int>(type: "INTEGER", nullable: false),
                    FollowersGained = table.Column<int>(type: "INTEGER", nullable: false),
                    FollowersLost = table.Column<int>(type: "INTEGER", nullable: false),
                    EventsCreated = table.Column<int>(type: "INTEGER", nullable: false),
                    NotificationsCreated = table.Column<int>(type: "INTEGER", nullable: false),
                    EventsSummaryJson = table.Column<string>(type: "TEXT", nullable: false),
                    CatchupSummary = table.Column<string>(type: "TEXT", nullable: false),
                    IsAcknowledged = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfflineSimulationResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfflineSimulationResults_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostVirality",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PostViralityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PostId = table.Column<Guid>(type: "TEXT", nullable: false),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<float>(type: "REAL", nullable: false),
                    TotalEngagement = table.Column<int>(type: "INTEGER", nullable: false),
                    Velocity = table.Column<float>(type: "REAL", nullable: false),
                    PeakVelocity = table.Column<float>(type: "REAL", nullable: false),
                    Reach = table.Column<int>(type: "INTEGER", nullable: false),
                    ShareCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ViralAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MassivelyViralAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeclinedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FirstViralThresholdCrossed = table.Column<int>(type: "INTEGER", nullable: true),
                    ControversyLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    HasControversyAnalysis = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostVirality", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostVirality_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    PostCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ActivePostCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SubscriberCount = table.Column<int>(type: "INTEGER", nullable: false),
                    IsVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.Id);
                    table.UniqueConstraint("AK_Topics_TopicId", x => x.TopicId);
                });

            migrationBuilder.CreateTable(
                name: "TopicSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    TopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubscribedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ViralityTransition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TransitionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PostId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FromState = table.Column<int>(type: "INTEGER", nullable: false),
                    ToState = table.Column<int>(type: "INTEGER", nullable: false),
                    ScoreAtTransition = table.Column<float>(type: "REAL", nullable: false),
                    EngagementAtTransition = table.Column<int>(type: "INTEGER", nullable: false),
                    VelocityAtTransition = table.Column<float>(type: "REAL", nullable: false),
                    TransitionedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Metadata = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViralityTransition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ViralityTransition_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Communities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CommunityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    OwnerAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Topic = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Visibility = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    MemberCount = table.Column<int>(type: "INTEGER", nullable: false),
                    PostCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TopicId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Communities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Communities_Accounts_OwnerAccountId",
                        column: x => x.OwnerAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Communities_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Hashtags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HashtagId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tag = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    NormalizedTag = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TopicId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UsageCount = table.Column<int>(type: "INTEGER", nullable: false),
                    TodayUsageCount = table.Column<int>(type: "INTEGER", nullable: false),
                    IsTrending = table.Column<bool>(type: "INTEGER", nullable: false),
                    TrendingSince = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TrendingRank = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hashtags", x => x.Id);
                    table.UniqueConstraint("AK_Hashtags_HashtagId", x => x.HashtagId);
                    table.ForeignKey(
                        name: "FK_Hashtags_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "TopicId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "NewsArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ArticleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NewsAccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Headline = table.Column<string>(type: "TEXT", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: false),
                    TagsJson = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    CoveredTopicId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CoveredRumorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CoveredEventId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RelatedPostId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MentionedAccountsJson = table.Column<string>(type: "TEXT", nullable: false),
                    Views = table.Column<int>(type: "INTEGER", nullable: false),
                    Shares = table.Column<int>(type: "INTEGER", nullable: false),
                    Comments = table.Column<int>(type: "INTEGER", nullable: false),
                    BreakingNewsBonus = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    IsBreakingNews = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsArticles", x => x.Id);
                    table.UniqueConstraint("AK_NewsArticles_ArticleId", x => x.ArticleId);
                    table.ForeignKey(
                        name: "FK_NewsArticles_NewsAccounts_NewsAccountId",
                        column: x => x.NewsAccountId,
                        principalTable: "NewsAccounts",
                        principalColumn: "NewsAccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NewsArticles_Topics_CoveredTopicId",
                        column: x => x.CoveredTopicId,
                        principalTable: "Topics",
                        principalColumn: "TopicId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CommunityMemberships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MembershipId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CommunityId = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunityMemberships_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunityMemberships_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    NarrativeContext = table.Column<string>(type: "TEXT", nullable: false),
                    CreatorAccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Visibility = table.Column<int>(type: "INTEGER", nullable: false),
                    Topic = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CommunityId = table.Column<int>(type: "INTEGER", nullable: true),
                    Popularity = table.Column<int>(type: "INTEGER", nullable: false),
                    ParticipantCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxParticipants = table.Column<int>(type: "INTEGER", nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", nullable: false),
                    DramaLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    FollowUpProbability = table.Column<double>(type: "REAL", nullable: false),
                    NarrativeArcLength = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentEventId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TriggerEventId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EventChainId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ChainDepth = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.UniqueConstraint("AK_Events_EventId", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_Events_Accounts_CreatorAccountId",
                        column: x => x.CreatorAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Events_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Rumors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RumorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OriginAccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    Claim = table.Column<string>(type: "TEXT", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", nullable: false),
                    TruthStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    SpreadType = table.Column<int>(type: "INTEGER", nullable: false),
                    SourcePostId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ShareCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SpreadByCount = table.Column<int>(type: "INTEGER", nullable: false),
                    PeakVelocity = table.Column<float>(type: "REAL", nullable: false),
                    FirstSeenAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PeakedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Analysis = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsNotable = table.Column<bool>(type: "INTEGER", nullable: false),
                    CommunityId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rumors", x => x.Id);
                    table.UniqueConstraint("AK_Rumors_RumorId", x => x.RumorId);
                    table.ForeignKey(
                        name: "FK_Rumors_Accounts_OriginAccountId",
                        column: x => x.OriginAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rumors_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Rumors_Posts_SourcePostId",
                        column: x => x.SourcePostId,
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Trends",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrendId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    TopicId = table.Column<Guid>(type: "TEXT", nullable: true),
                    HashtagId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Query = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Strength = table.Column<int>(type: "INTEGER", nullable: false),
                    PostCount = table.Column<int>(type: "INTEGER", nullable: false),
                    UniquePosters = table.Column<int>(type: "INTEGER", nullable: false),
                    EngagementTotal = table.Column<int>(type: "INTEGER", nullable: false),
                    Velocity = table.Column<float>(type: "REAL", nullable: false),
                    Rank = table.Column<int>(type: "INTEGER", nullable: false),
                    Scope = table.Column<int>(type: "INTEGER", nullable: false),
                    CommunityId = table.Column<int>(type: "INTEGER", nullable: true),
                    CalculatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PeakedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trends", x => x.Id);
                    table.UniqueConstraint("AK_Trends_TrendId", x => x.TrendId);
                    table.ForeignKey(
                        name: "FK_Trends_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Trends_Hashtags_HashtagId",
                        column: x => x.HashtagId,
                        principalTable: "Hashtags",
                        principalColumn: "HashtagId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Trends_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "TopicId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "NewsExposures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExposureId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ArticleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CommunityId = table.Column<int>(type: "INTEGER", nullable: false),
                    Views = table.Column<int>(type: "INTEGER", nullable: false),
                    ExposedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsExposures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsExposures_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NewsExposures_NewsArticles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "NewsArticles",
                        principalColumn: "ArticleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CausalChain",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CausalChainId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CauseEventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CauseType = table.Column<int>(type: "INTEGER", nullable: false),
                    CauseDescription = table.Column<string>(type: "TEXT", nullable: false),
                    CauseStrength = table.Column<double>(type: "REAL", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Metadata = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CausalChain", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CausalChain_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CausalChain_Events_CauseEventId",
                        column: x => x.CauseEventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CausalChain_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventConsequences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EventConsequenceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Parameters = table.Column<string>(type: "TEXT", nullable: false),
                    WasExecuted = table.Column<bool>(type: "INTEGER", nullable: false),
                    FailureReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventConsequences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventConsequences_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventParticipations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EventParticipationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventId = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ContributionScore = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    LLMReasoning = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventParticipations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventParticipations_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventParticipations_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountBeliefs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountBeliefId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    RumorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Belief = table.Column<int>(type: "INTEGER", nullable: false),
                    Confidence = table.Column<double>(type: "REAL", nullable: false),
                    FormedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    InfluenceSource = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountBeliefs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountBeliefs_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountBeliefs_Rumors_RumorId",
                        column: x => x.RumorId,
                        principalTable: "Rumors",
                        principalColumn: "RumorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RumorEvidence",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EvidenceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RumorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    SourceUrl = table.Column<string>(type: "TEXT", nullable: true),
                    SupportsRumor = table.Column<bool>(type: "INTEGER", nullable: false),
                    CredibilityScore = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RumorEvidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RumorEvidence_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RumorEvidence_Rumors_RumorId",
                        column: x => x.RumorId,
                        principalTable: "Rumors",
                        principalColumn: "RumorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrendPropagations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PropagationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TrendId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FromCommunityId = table.Column<int>(type: "INTEGER", nullable: false),
                    ToCommunityId = table.Column<int>(type: "INTEGER", nullable: false),
                    PropagatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrendPropagations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrendPropagations_Communities_FromCommunityId",
                        column: x => x.FromCommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrendPropagations_Communities_ToCommunityId",
                        column: x => x.ToCommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrendPropagations_Trends_TrendId",
                        column: x => x.TrendId,
                        principalTable: "Trends",
                        principalColumn: "TrendId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CommunityId",
                table: "Posts",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_Topic",
                table: "Posts",
                column: "Topic");

            migrationBuilder.CreateIndex(
                name: "IX_AccountBeliefs_AccountBeliefId",
                table: "AccountBeliefs",
                column: "AccountBeliefId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountBeliefs_AccountId_RumorId",
                table: "AccountBeliefs",
                columns: new[] { "AccountId", "RumorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountBeliefs_RumorId",
                table: "AccountBeliefs",
                column: "RumorId");

            migrationBuilder.CreateIndex(
                name: "IX_AiProviderConfigs_Provider",
                table: "AiProviderConfigs",
                column: "Provider",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_Key",
                table: "AppSettings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CausalChain_AccountId",
                table: "CausalChain",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CausalChain_CauseEventId",
                table: "CausalChain",
                column: "CauseEventId");

            migrationBuilder.CreateIndex(
                name: "IX_CausalChain_EventId",
                table: "CausalChain",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Communities_IsActive",
                table: "Communities",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Communities_MemberCount",
                table: "Communities",
                column: "MemberCount");

            migrationBuilder.CreateIndex(
                name: "IX_Communities_OwnerAccountId",
                table: "Communities",
                column: "OwnerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Communities_Slug",
                table: "Communities",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Communities_Topic",
                table: "Communities",
                column: "Topic");

            migrationBuilder.CreateIndex(
                name: "IX_Communities_TopicId",
                table: "Communities",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMemberships_AccountId_IsActive",
                table: "CommunityMemberships",
                columns: new[] { "AccountId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMemberships_CommunityId_AccountId",
                table: "CommunityMemberships",
                columns: new[] { "CommunityId", "AccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMemberships_CommunityId_IsActive",
                table: "CommunityMemberships",
                columns: new[] { "CommunityId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EventConsequences_EventId",
                table: "EventConsequences",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventConsequences_ProcessedAt",
                table: "EventConsequences",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EventConsequences_Type",
                table: "EventConsequences",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_EventParticipations_AccountId",
                table: "EventParticipations",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_EventParticipations_EventId_AccountId",
                table: "EventParticipations",
                columns: new[] { "EventId", "AccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_CommunityId",
                table: "Events",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_CreatorAccountId",
                table: "Events",
                column: "CreatorAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventChainId",
                table: "Events",
                column: "EventChainId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_IsDeleted",
                table: "Events",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ParentEventId",
                table: "Events",
                column: "ParentEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_Status_CreatedAt",
                table: "Events",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_Topic",
                table: "Events",
                column: "Topic");

            migrationBuilder.CreateIndex(
                name: "IX_Events_TriggerEventId",
                table: "Events",
                column: "TriggerEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_Type_Status",
                table: "Events",
                columns: new[] { "Type", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FeedImpressions_AccountId",
                table: "FeedImpressions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedImpressions_AccountId_CreatedAt",
                table: "FeedImpressions",
                columns: new[] { "AccountId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FeedImpressions_AccountId_PostId",
                table: "FeedImpressions",
                columns: new[] { "AccountId", "PostId" });

            migrationBuilder.CreateIndex(
                name: "IX_FeedImpressions_PostId",
                table: "FeedImpressions",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_Hashtags_IsTrending",
                table: "Hashtags",
                column: "IsTrending");

            migrationBuilder.CreateIndex(
                name: "IX_Hashtags_NormalizedTag",
                table: "Hashtags",
                column: "NormalizedTag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hashtags_Tag",
                table: "Hashtags",
                column: "Tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hashtags_TodayUsageCount",
                table: "Hashtags",
                column: "TodayUsageCount");

            migrationBuilder.CreateIndex(
                name: "IX_Hashtags_TopicId",
                table: "Hashtags",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsAccounts_AccountId",
                table: "NewsAccounts",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsAccounts_Category",
                table: "NewsAccounts",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_NewsAccounts_IsActive",
                table: "NewsAccounts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_NewsAccounts_NewsAccountId",
                table: "NewsAccounts",
                column: "NewsAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_ArticleId",
                table: "NewsArticles",
                column: "ArticleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_Category",
                table: "NewsArticles",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_CoveredTopicId",
                table: "NewsArticles",
                column: "CoveredTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_IsBreakingNews",
                table: "NewsArticles",
                column: "IsBreakingNews");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_NewsAccountId",
                table: "NewsArticles",
                column: "NewsAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_PublishedAt",
                table: "NewsArticles",
                column: "PublishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_Status",
                table: "NewsArticles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_NewsExposures_ArticleId",
                table: "NewsExposures",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsExposures_CommunityId",
                table: "NewsExposures",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsExposures_ExposureId",
                table: "NewsExposures",
                column: "ExposureId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ActorAccountId",
                table: "Notifications",
                column: "ActorAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientAccountId_CreatedAt",
                table: "Notifications",
                columns: new[] { "RecipientAccountId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientAccountId_IsRead",
                table: "Notifications",
                columns: new[] { "RecipientAccountId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelatedPostId",
                table: "Notifications",
                column: "RelatedPostId");

            migrationBuilder.CreateIndex(
                name: "IX_OfflineSimulationResults_AccountId",
                table: "OfflineSimulationResults",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_OfflineSimulationResults_AccountId_IsAcknowledged",
                table: "OfflineSimulationResults",
                columns: new[] { "AccountId", "IsAcknowledged" });

            migrationBuilder.CreateIndex(
                name: "IX_OfflineSimulationResults_CreatedAt",
                table: "OfflineSimulationResults",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PostVirality_LastUpdated",
                table: "PostVirality",
                column: "LastUpdated");

            migrationBuilder.CreateIndex(
                name: "IX_PostVirality_PostId",
                table: "PostVirality",
                column: "PostId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostVirality_Score",
                table: "PostVirality",
                column: "Score");

            migrationBuilder.CreateIndex(
                name: "IX_PostVirality_State",
                table: "PostVirality",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_PostVirality_Velocity",
                table: "PostVirality",
                column: "Velocity");

            migrationBuilder.CreateIndex(
                name: "IX_RumorEvidence_AccountId",
                table: "RumorEvidence",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RumorEvidence_EvidenceId",
                table: "RumorEvidence",
                column: "EvidenceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RumorEvidence_RumorId",
                table: "RumorEvidence",
                column: "RumorId");

            migrationBuilder.CreateIndex(
                name: "IX_Rumors_CommunityId",
                table: "Rumors",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Rumors_FirstSeenAt",
                table: "Rumors",
                column: "FirstSeenAt");

            migrationBuilder.CreateIndex(
                name: "IX_Rumors_IsActive",
                table: "Rumors",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Rumors_OriginAccountId",
                table: "Rumors",
                column: "OriginAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Rumors_RumorId",
                table: "Rumors",
                column: "RumorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rumors_SourcePostId",
                table: "Rumors",
                column: "SourcePostId");

            migrationBuilder.CreateIndex(
                name: "IX_Rumors_TruthStatus",
                table: "Rumors",
                column: "TruthStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_Category",
                table: "Topics",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_IsActive",
                table: "Topics",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_Name",
                table: "Topics",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Topics_Slug",
                table: "Topics",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TopicSubscriptions_AccountId_TopicId",
                table: "TopicSubscriptions",
                columns: new[] { "AccountId", "TopicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrendPropagations_FromCommunityId",
                table: "TrendPropagations",
                column: "FromCommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_TrendPropagations_ToCommunityId",
                table: "TrendPropagations",
                column: "ToCommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_TrendPropagations_TrendId",
                table: "TrendPropagations",
                column: "TrendId");

            migrationBuilder.CreateIndex(
                name: "IX_Trends_CommunityId",
                table: "Trends",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Trends_ExpiresAt",
                table: "Trends",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_Trends_HashtagId",
                table: "Trends",
                column: "HashtagId");

            migrationBuilder.CreateIndex(
                name: "IX_Trends_IsActive",
                table: "Trends",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Trends_Query",
                table: "Trends",
                column: "Query");

            migrationBuilder.CreateIndex(
                name: "IX_Trends_Rank",
                table: "Trends",
                column: "Rank");

            migrationBuilder.CreateIndex(
                name: "IX_Trends_Scope",
                table: "Trends",
                column: "Scope");

            migrationBuilder.CreateIndex(
                name: "IX_Trends_Strength",
                table: "Trends",
                column: "Strength");

            migrationBuilder.CreateIndex(
                name: "IX_Trends_TopicId",
                table: "Trends",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_ViralityTransition_PostId",
                table: "ViralityTransition",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_ViralityTransition_PostId_TransitionedAt",
                table: "ViralityTransition",
                columns: new[] { "PostId", "TransitionedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ViralityTransition_TransitionedAt",
                table: "ViralityTransition",
                column: "TransitionedAt");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Communities_CommunityId",
                table: "Posts",
                column: "CommunityId",
                principalTable: "Communities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Communities_CommunityId",
                table: "Posts");

            migrationBuilder.DropTable(
                name: "AccountBeliefs");

            migrationBuilder.DropTable(
                name: "AiProviderConfigs");

            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "CausalChain");

            migrationBuilder.DropTable(
                name: "CommunityMemberships");

            migrationBuilder.DropTable(
                name: "EventConsequences");

            migrationBuilder.DropTable(
                name: "EventParticipations");

            migrationBuilder.DropTable(
                name: "FeedImpressions");

            migrationBuilder.DropTable(
                name: "NewsExposures");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OfflineSimulationResults");

            migrationBuilder.DropTable(
                name: "PostVirality");

            migrationBuilder.DropTable(
                name: "RumorEvidence");

            migrationBuilder.DropTable(
                name: "TopicSubscriptions");

            migrationBuilder.DropTable(
                name: "TrendPropagations");

            migrationBuilder.DropTable(
                name: "ViralityTransition");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "NewsArticles");

            migrationBuilder.DropTable(
                name: "Rumors");

            migrationBuilder.DropTable(
                name: "Trends");

            migrationBuilder.DropTable(
                name: "NewsAccounts");

            migrationBuilder.DropTable(
                name: "Communities");

            migrationBuilder.DropTable(
                name: "Hashtags");

            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Posts_PostId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_CommunityId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_Topic",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "FameLevel",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "FollowerCount",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "CommunityId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Topic",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "LastSeenAt",
                table: "Accounts");
        }
    }
}
