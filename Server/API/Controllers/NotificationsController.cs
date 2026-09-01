using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Contracts.Responses;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.API.Controllers;

public static class NotificationsController
{
    private const int MaxPostSnippetLength = 100;

    public static void MapNotificationEndpoints(this WebApplication app)
    {
        // Get notifications for authenticated user
        app.MapGet("/api/notifications", GetNotifications)
            .WithTags("Notifications")
            .WithName("GetNotifications")
            .RequireAuthorization();

        // Get unread count
        app.MapGet("/api/notifications/unread-count", GetUnreadCount)
            .WithTags("Notifications")
            .WithName("GetUnreadCount")
            .RequireAuthorization();

        // Mark single notification as read
        app.MapPost("/api/notifications/{notificationId:guid}/read", MarkAsRead)
            .WithTags("Notifications")
            .WithName("MarkNotificationAsRead")
            .RequireAuthorization();

        // Mark all notifications as read
        app.MapPost("/api/notifications/read-all", MarkAllAsRead)
            .WithTags("Notifications")
            .WithName("MarkAllNotificationsAsRead")
            .RequireAuthorization();
    }

    private static async Task<IResult> GetNotifications(
        ClaimsPrincipal user,
        [FromQuery] string? cursor,
        [FromQuery] int pageSize,
        INotificationService notificationService)
    {
        // Get authenticated user
        var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var accountId))
        {
            return Results.Unauthorized();
        }

        // Validate page size
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        // Get notifications
        var (items, nextCursor) = await notificationService.GetNotificationsAsync(accountId, cursor, pageSize);

        // Map to response DTOs
        var notifications = items.Select(n => new NotificationResponse(
            n.Id,
            n.Type.ToString(),
            n.ActorAccount?.AccountId ?? Guid.Empty,
            n.ActorAccount?.Username ?? "Unknown",
            n.ActorAccount?.Profile?.DisplayName ?? n.ActorAccount?.Username ?? "Unknown",
            n.ActorAccount?.Profile?.AvatarUrl,
            n.RelatedPostId.HasValue ? n.RelatedPost?.PostId : null,
            GetPostSnippet(n),
            IsPostDeleted(n),
            n.CreatedAt,
            n.IsRead,
            n.ReadAt
        ));

        return Results.Ok(new PaginatedNotificationsResponse(
            notifications,
            nextCursor,
            nextCursor != null
        ));
    }

    private static async Task<IResult> GetUnreadCount(
        ClaimsPrincipal user,
        INotificationService notificationService)
    {
        // Get authenticated user
        var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var accountId))
        {
            return Results.Unauthorized();
        }

        var count = await notificationService.GetUnreadCountAsync(accountId);

        return Results.Ok(new UnreadCountResponse(count));
    }

    private static async Task<IResult> MarkAsRead(
        Guid notificationId,
        ClaimsPrincipal user,
        INotificationService notificationService)
    {
        // Get authenticated user
        var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var accountId))
        {
            return Results.Unauthorized();
        }

        // Verify ownership
        var notification = await notificationService.GetByIdAsync(notificationId);
        if (notification == null)
        {
            return Results.NotFound(new ErrorResponse("Notification not found"));
        }

        if (notification.RecipientAccountId != accountId)
        {
            return Results.Forbid();
        }

        var success = await notificationService.MarkAsReadAsync(accountId, notificationId);

        return Results.Ok(new MarkReadResponse(success, success ? "Notification marked as read" : "Notification already read"));
    }

    private static async Task<IResult> MarkAllAsRead(
        ClaimsPrincipal user,
        INotificationService notificationService)
    {
        // Get authenticated user
        var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var accountId))
        {
            return Results.Unauthorized();
        }

        var count = await notificationService.MarkAllAsReadAsync(accountId);

        return Results.Ok(new MarkAllReadResponse(count));
    }

    private static string? GetPostSnippet(Notification notification)
    {
        if (!notification.RelatedPostId.HasValue)
        {
            return null;
        }

        // If post was soft-deleted, return null (handled by IsPostDeleted)
        if (notification.RelatedPost == null)
        {
            return null;
        }

        var content = notification.RelatedPost.Content;
        if (string.IsNullOrEmpty(content))
        {
            return null;
        }

        // Truncate to reasonable snippet length
        if (content.Length > MaxPostSnippetLength)
        {
            return content.Substring(0, MaxPostSnippetLength) + "...";
        }

        return content;
    }

    private static bool IsPostDeleted(Notification notification)
    {
        // If there's a related post but it's not loaded, we can't determine deletion
        // If RelatedPostId is set but RelatedPost is null, the post was deleted
        return notification.RelatedPostId.HasValue && notification.RelatedPost == null;
    }
}
