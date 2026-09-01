using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;
using SocialMediaSimulator.Server.Application.Services;
using Xunit;

namespace SocialMediaSimulator.Tests;

public class NotificationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly NotificationService _service;
    private readonly Mock<ILogger<NotificationService>> _mockLogger;

    public NotificationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _mockLogger = new Mock<ILogger<NotificationService>>();
        _service = new NotificationService(_context, _mockLogger.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task<(Account follower, Account followed)> CreateFollowAccountsAsync()
    {
        var follower = new Account
        {
            Username = "follower",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        var followed = new Account
        {
            Username = "followed",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Accounts.AddRange(follower, followed);
        await _context.SaveChangesAsync();
        
        return (follower, followed);
    }

    private async Task<(Account liker, Post post)> CreateLikeScenarioAsync()
    {
        var liker = new Account
        {
            Username = "liker",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        var author = new Account
        {
            Username = "author",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Accounts.AddRange(liker, author);
        await _context.SaveChangesAsync();
        
        var post = new Post
        {
            AuthorAccountId = author.Id,
            Content = "Test post content",
            Status = PostStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        
        return (liker, post);
    }

    private async Task<(Account commenter, Post post)> CreateCommentScenarioAsync()
    {
        var commenter = new Account
        {
            Username = "commenter",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        var author = new Account
        {
            Username = "postauthor",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Accounts.AddRange(commenter, author);
        await _context.SaveChangesAsync();
        
        var post = new Post
        {
            AuthorAccountId = author.Id,
            Content = "Test post for comments",
            Status = PostStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        
        return (commenter, post);
    }

    #region Follow Notification Tests

    [Fact]
    public async Task NotifyFollow_CreatesNotification()
    {
        // Arrange
        var (follower, followed) = await CreateFollowAccountsAsync();
        
        var follow = new Follow
        {
            FollowerAccountId = follower.Id,
            FollowedAccountId = followed.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();

        // Act
        await _service.NotifyFollowAsync(follow.Id, follower.Id, followed.Id);

        // Assert
        var notification = await _context.Notifications.FirstOrDefaultAsync();
        Assert.NotNull(notification);
        Assert.Equal(followed.Id, notification.RecipientAccountId);
        Assert.Equal(follower.Id, notification.ActorAccountId);
        Assert.Equal(NotificationType.Follow, notification.Type);
        Assert.False(notification.IsRead);
    }

    [Fact]
    public async Task NotifyFollow_SuppressesSelfNotification()
    {
        // Arrange
        var account = new Account
        {
            Username = "single",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        
        var follow = new Follow
        {
            FollowerAccountId = account.Id,
            FollowedAccountId = account.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();

        // Act
        await _service.NotifyFollowAsync(follow.Id, account.Id, account.Id);

        // Assert
        var count = await _context.Notifications.CountAsync();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task NotifyFollow_SuppressedWhenRecipientBlockedActor()
    {
        // Arrange
        var (follower, followed) = await CreateFollowAccountsAsync();
        
        // Recipient blocks the actor
        _context.Blocks.Add(new Block
        {
            BlockerAccountId = followed.Id,
            BlockedAccountId = follower.Id,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        
        var follow = new Follow
        {
            FollowerAccountId = follower.Id,
            FollowedAccountId = followed.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();

        // Act
        await _service.NotifyFollowAsync(follow.Id, follower.Id, followed.Id);

        // Assert
        var count = await _context.Notifications.CountAsync();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task NotifyFollow_SuppressedWhenActorBlockedRecipient()
    {
        // Arrange
        var (follower, followed) = await CreateFollowAccountsAsync();
        
        // Actor blocks the recipient
        _context.Blocks.Add(new Block
        {
            BlockerAccountId = follower.Id,
            BlockedAccountId = followed.Id,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        
        var follow = new Follow
        {
            FollowerAccountId = follower.Id,
            FollowedAccountId = followed.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();

        // Act
        await _service.NotifyFollowAsync(follow.Id, follower.Id, followed.Id);

        // Assert
        var count = await _context.Notifications.CountAsync();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task NotifyFollow_SuppressedWhenRecipientMutedActor()
    {
        // Arrange
        var (follower, followed) = await CreateFollowAccountsAsync();
        
        // Recipient mutes the actor
        _context.Mutes.Add(new Mute
        {
            MuterAccountId = followed.Id,
            MutedAccountId = follower.Id,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        
        var follow = new Follow
        {
            FollowerAccountId = follower.Id,
            FollowedAccountId = followed.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();

        // Act
        await _service.NotifyFollowAsync(follow.Id, follower.Id, followed.Id);

        // Assert
        var count = await _context.Notifications.CountAsync();
        Assert.Equal(0, count);
    }

    #endregion

    #region Like Notification Tests

    [Fact]
    public async Task NotifyLike_CreatesNotification()
    {
        // Arrange
        var (liker, post) = await CreateLikeScenarioAsync();
        
        var like = new PostLike
        {
            PostId = post.Id,
            AccountId = liker.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.PostLikes.Add(like);
        await _context.SaveChangesAsync();

        // Act
        await _service.NotifyLikeAsync(like.Id, liker.Id, post.AuthorAccountId, post.Id);

        // Assert
        var notification = await _context.Notifications.FirstOrDefaultAsync();
        Assert.NotNull(notification);
        Assert.Equal(post.AuthorAccountId, notification.RecipientAccountId);
        Assert.Equal(liker.Id, notification.ActorAccountId);
        Assert.Equal(NotificationType.Like, notification.Type);
        Assert.Equal(post.Id, notification.RelatedPostId);
        Assert.False(notification.IsRead);
    }

    [Fact]
    public async Task NotifyLike_SuppressesSelfLike()
    {
        // Arrange
        var account = new Account
        {
            Username = "selfliker",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        
        var post = new Post
        {
            AuthorAccountId = account.Id,
            Content = "My post",
            Status = PostStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        
        var like = new PostLike
        {
            PostId = post.Id,
            AccountId = account.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.PostLikes.Add(like);
        await _context.SaveChangesAsync();

        // Act
        await _service.NotifyLikeAsync(like.Id, account.Id, account.Id, post.Id);

        // Assert
        var count = await _context.Notifications.CountAsync();
        Assert.Equal(0, count);
    }

    #endregion

    #region Comment Notification Tests

    [Fact]
    public async Task NotifyComment_CreatesNotification()
    {
        // Arrange
        var (commenter, post) = await CreateCommentScenarioAsync();
        
        var comment = new Comment
        {
            PostId = post.Id,
            AuthorAccountId = commenter.Id,
            Content = "Great post!",
            Status = CommentStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // Act
        await _service.NotifyCommentAsync(comment.Id, commenter.Id, post.AuthorAccountId, post.Id);

        // Assert
        var notification = await _context.Notifications.FirstOrDefaultAsync();
        Assert.NotNull(notification);
        Assert.Equal(post.AuthorAccountId, notification.RecipientAccountId);
        Assert.Equal(commenter.Id, notification.ActorAccountId);
        Assert.Equal(NotificationType.Comment, notification.Type);
        Assert.Equal(post.Id, notification.RelatedPostId);
        Assert.False(notification.IsRead);
    }

    #endregion

    #region Unread Count Tests

    [Fact]
    public async Task GetUnreadCount_ReturnsCorrectCount()
    {
        // Arrange
        var (follower, followed) = await CreateFollowAccountsAsync();
        
        var follow = new Follow
        {
            FollowerAccountId = follower.Id,
            FollowedAccountId = followed.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();
        
        await _service.NotifyFollowAsync(follow.Id, follower.Id, followed.Id);
        
        var (liker, post) = await CreateLikeScenarioAsync();
        var like = new PostLike
        {
            PostId = post.Id,
            AccountId = liker.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.PostLikes.Add(like);
        await _context.SaveChangesAsync();
        await _service.NotifyLikeAsync(like.Id, liker.Id, post.AuthorAccountId, post.Id);

        // Act
        var count = await _service.GetUnreadCountAsync(followed.Id);

        // Assert
        Assert.Equal(1, count);
    }

    #endregion

    #region Mark as Read Tests

    [Fact]
    public async Task MarkAsRead_MarksNotificationAsRead()
    {
        // Arrange
        var (follower, followed) = await CreateFollowAccountsAsync();
        
        var follow = new Follow
        {
            FollowerAccountId = follower.Id,
            FollowedAccountId = followed.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();
        
        await _service.NotifyFollowAsync(follow.Id, follower.Id, followed.Id);
        var notification = await _context.Notifications.FirstAsync();
        
        // Act
        var result = await _service.MarkAsReadAsync(followed.Id, notification.Id);

        // Assert
        Assert.True(result);
        
        var updated = await _context.Notifications.FindAsync(notification.Id);
        Assert.True(updated!.IsRead);
        Assert.NotNull(updated.ReadAt);
    }

    [Fact]
    public async Task MarkAsRead_ReturnsFalseForNonExistentNotification()
    {
        // Arrange
        var (account1, account2) = await CreateFollowAccountsAsync();
        
        // Act
        var result = await _service.MarkAsReadAsync(account1.Id, Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task MarkAsRead_RejectsDifferentUser()
    {
        // Arrange
        var (follower, followed) = await CreateFollowAccountsAsync();
        
        var thirdParty = new Account
        {
            Username = "thirdparty",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        _context.Accounts.Add(thirdParty);
        await _context.SaveChangesAsync();
        
        var follow = new Follow
        {
            FollowerAccountId = follower.Id,
            FollowedAccountId = followed.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();
        
        await _service.NotifyFollowAsync(follow.Id, follower.Id, followed.Id);
        var notification = await _context.Notifications.FirstAsync();
        
        // Act - try to mark with wrong user
        var result = await _service.MarkAsReadAsync(thirdParty.Id, notification.Id);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task MarkAllAsRead_MarksAllNotificationsAsRead()
    {
        // Arrange
        var (follower, followed) = await CreateFollowAccountsAsync();
        
        var follow = new Follow
        {
            FollowerAccountId = follower.Id,
            FollowedAccountId = followed.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();
        
        await _service.NotifyFollowAsync(follow.Id, follower.Id, followed.Id);
        
        var (liker, post) = await CreateLikeScenarioAsync();
        var like = new PostLike
        {
            PostId = post.Id,
            AccountId = liker.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.PostLikes.Add(like);
        await _context.SaveChangesAsync();
        await _service.NotifyLikeAsync(like.Id, liker.Id, post.AuthorAccountId, post.Id);

        // Act
        var count = await _service.MarkAllAsReadAsync(followed.Id);

        // Assert
        Assert.Equal(1, count);
        
        var unreadCount = await _service.GetUnreadCountAsync(followed.Id);
        Assert.Equal(0, unreadCount);
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public async Task GetNotifications_ReturnsPaginatedResults()
    {
        // Arrange
        var (follower1, followed) = await CreateFollowAccountsAsync();
        
        // Create multiple notifications
        for (int i = 0; i < 5; i++)
        {
            var follower = new Account
            {
                Username = $"follower{i}",
                PasswordHash = "hash",
                AccountType = AccountType.OrdinaryUser,
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow
            };
            _context.Accounts.Add(follower);
            await _context.SaveChangesAsync();
            
            var follow = new Follow
            {
                FollowerAccountId = follower.Id,
                FollowedAccountId = followed.Id,
                CreatedAt = DateTime.UtcNow.AddMinutes(i)
            };
            _context.Follows.Add(follow);
            await _context.SaveChangesAsync();
            
            await _service.NotifyFollowAsync(follow.Id, follower.Id, followed.Id);
        }

        // Act
        var (items, nextCursor) = await _service.GetNotificationsAsync(followed.Id, null, 2);

        // Assert
        Assert.Equal(2, items.Count());
        Assert.NotNull(nextCursor);
    }

    [Fact]
    public async Task GetNotifications_ReturnsNewestFirst()
    {
        // Arrange
        var (follower1, followed) = await CreateFollowAccountsAsync();
        
        // Create notifications with different timestamps
        var follower2 = new Account
        {
            Username = "follower2",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        _context.Accounts.Add(follower2);
        await _context.SaveChangesAsync();
        
        // First notification
        var follow1 = new Follow
        {
            FollowerAccountId = follower1.Id,
            FollowedAccountId = followed.Id,
            CreatedAt = DateTime.UtcNow.AddMinutes(-1)
        };
        _context.Follows.Add(follow1);
        await _context.SaveChangesAsync();
        await _service.NotifyFollowAsync(follow1.Id, follower1.Id, followed.Id);
        
        // Second notification (more recent)
        var follow2 = new Follow
        {
            FollowerAccountId = follower2.Id,
            FollowedAccountId = followed.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Follows.Add(follow2);
        await _context.SaveChangesAsync();
        await _service.NotifyFollowAsync(follow2.Id, follower2.Id, followed.Id);

        // Act
        var (items, _) = await _service.GetNotificationsAsync(followed.Id, null, 10);

        // Assert
        var list = items.ToList();
        Assert.Equal(2, list.Count);
        Assert.Equal(follower2.Id, list[0].ActorAccountId); // Most recent first
        Assert.Equal(follower1.Id, list[1].ActorAccountId);
    }

    #endregion

    #region Deleted Post Handling Tests

    [Fact]
    public async Task GetNotifications_HandlesDeletedPostGracefully()
    {
        // Arrange
        var (commenter, post) = await CreateCommentScenarioAsync();
        
        var comment = new Comment
        {
            PostId = post.Id,
            AuthorAccountId = commenter.Id,
            Content = "Great post!",
            Status = CommentStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        
        await _service.NotifyCommentAsync(comment.Id, commenter.Id, post.AuthorAccountId, post.Id);
        var notification = await _context.Notifications.FirstAsync();
        
        // Delete the post (soft delete)
        post.Status = PostStatus.Deleted;
        await _context.SaveChangesAsync();
        
        // Reload notification with fresh context (simulating separate API call)
        var freshService = new NotificationService(_context, null!);
        
        // Act
        var fetchedNotification = await freshService.GetByIdAsync(notification.Id);

        // Assert - notification should still exist
        Assert.NotNull(fetchedNotification);
        // RelatedPost should be null (soft deleted)
        Assert.True(fetchedNotification.RelatedPost == null || fetchedNotification.RelatedPost?.Status == PostStatus.Deleted);
    }

    #endregion

    #region NPC Attribution Tests

    [Fact]
    public async Task NotifyFollow_SupportsNpcActor()
    {
        // Arrange - NPC is the actor (follower), human is the recipient
        var npc = new Account
        {
            Username = "npc_bot_001",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        var human = new Account
        {
            Username = "human_user",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Accounts.AddRange(npc, human);
        await _context.SaveChangesAsync();
        
        var follow = new Follow
        {
            FollowerAccountId = npc.Id,
            FollowedAccountId = human.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();

        // Act
        await _service.NotifyFollowAsync(follow.Id, npc.Id, human.Id);

        // Assert
        var notification = await _context.Notifications.FirstOrDefaultAsync();
        Assert.NotNull(notification);
        Assert.Equal(human.Id, notification.RecipientAccountId);
        Assert.Equal(npc.Id, notification.ActorAccountId);
    }

    #endregion
}

public class NotificationIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly SocialGraphService _socialGraphService;
    private readonly PostService _postService;
    private readonly NotificationService _notificationService;
    private readonly Mock<ILogger<NotificationService>> _mockLogger;

    public NotificationIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _mockLogger = new Mock<ILogger<NotificationService>>();
        _notificationService = new NotificationService(_context, _mockLogger.Object);
        _socialGraphService = new SocialGraphService(_context, _notificationService);
        _postService = new PostService(_context, _notificationService);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task SocialGraphService_FollowCreatesNotification()
    {
        // Arrange
        var follower = new Account
        {
            Username = "follower",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        var followed = new Account
        {
            Username = "followed",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Accounts.AddRange(follower, followed);
        await _context.SaveChangesAsync();

        // Act
        var follow = await _socialGraphService.FollowAsync(follower.Id, followed.Id);

        // Assert - the follow was created, but notification is fire-and-forget
        // so we need to wait for the background task
        Assert.NotNull(follow);
        
        // Wait for the background notification task to complete
        await Task.Delay(500);
        
        var notification = await _context.Notifications.FirstOrDefaultAsync();
        Assert.NotNull(notification);
        Assert.Equal(NotificationType.Follow, notification.Type);
    }

    [Fact]
    public async Task PostService_LikeCreatesNotification()
    {
        // Arrange
        var liker = new Account
        {
            Username = "liker",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        var author = new Account
        {
            Username = "author",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Accounts.AddRange(liker, author);
        await _context.SaveChangesAsync();
        
        var post = new Post
        {
            AuthorAccountId = author.Id,
            Content = "Test post",
            Status = PostStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Act
        var like = await _postService.LikePostAsync(liker.Id, post.PostId);

        // Assert - give fire-and-forget task time to complete
        await Task.Delay(100);
        
        var notification = await _context.Notifications.FirstOrDefaultAsync();
        Assert.NotNull(notification);
        Assert.Equal(NotificationType.Like, notification.Type);
    }

    [Fact]
    public async Task PostService_CommentCreatesNotification()
    {
        // Arrange
        var commenter = new Account
        {
            Username = "commenter",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        var author = new Account
        {
            Username = "author",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Accounts.AddRange(commenter, author);
        await _context.SaveChangesAsync();
        
        var post = new Post
        {
            AuthorAccountId = author.Id,
            Content = "Test post",
            Status = PostStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Act
        var comment = await _postService.CreateCommentAsync(commenter.Id, post.PostId, "Great post!");

        // Assert - give fire-and-forget task time to complete
        await Task.Delay(100);
        
        var notification = await _context.Notifications.FirstOrDefaultAsync();
        Assert.NotNull(notification);
        Assert.Equal(NotificationType.Comment, notification.Type);
    }

    [Fact]
    public async Task FollowNotification_SuppressedWhenBlocked()
    {
        // Note: Due to SocialGraphService blocking logic, if Alice blocks Bob,
        // Bob cannot follow Alice (follow is prevented). So this test verifies
        // the block check in NotificationService is consistent with SocialGraphService.
        // For notification suppression by block, we test the case where:
        // Alice blocks Bob, but then we add a follow directly (bypassing the block check)
        // and verify no notification is created.
        
        // Arrange
        var blocker = new Account
        {
            Username = "blocker",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        var blocked = new Account
        {
            Username = "blocked",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Accounts.AddRange(blocker, blocked);
        await _context.SaveChangesAsync();
        
        // Block: blocker blocks blocked
        _context.Blocks.Add(new Block
        {
            BlockerAccountId = blocker.Id,
            BlockedAccountId = blocked.Id,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        
        // Create a follow directly (bypassing SocialGraphService block check)
        // This simulates a follow that somehow got created despite the block
        var follow = new Follow
        {
            FollowerAccountId = blocked.Id,
            FollowedAccountId = blocker.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();

        // Act - notify about the follow
        await _notificationService.NotifyFollowAsync(follow.Id, blocked.Id, blocker.Id);

        // Assert - notification should be suppressed because blocker blocked blocked
        var notificationCount = await _context.Notifications.CountAsync();
        Assert.Equal(0, notificationCount);
    }

    [Fact]
    public async Task FollowNotification_SuppressedWhenMuted()
    {
        // Arrange
        var muter = new Account
        {
            Username = "muter",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        var muted = new Account
        {
            Username = "muted",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Accounts.AddRange(muter, muted);
        await _context.SaveChangesAsync();
        
        // Mute in advance
        await _socialGraphService.MuteAsync(muter.Id, muted.Id);
        
        // Clear notifications
        _context.Notifications.RemoveRange(_context.Notifications);
        await _context.SaveChangesAsync();

        // Act
        await _socialGraphService.FollowAsync(muted.Id, muter.Id);

        // Assert - give fire-and-forget task time to complete
        await Task.Delay(100);
        
        var notificationCount = await _context.Notifications.CountAsync();
        Assert.Equal(0, notificationCount);
    }
}
