using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Contracts.Responses;

namespace SocialMediaSimulator.Server.API.Controllers;

public static class PostsController
{
    public static void MapPostEndpoints(this WebApplication app)
    {
        // Create post (authenticated)
        app.MapPost("/api/posts", CreatePost)
            .WithTags("Posts")
            .WithName("CreatePost")
            .RequireAuthorization();

        // Get post (public)
        app.MapGet("/api/posts/{postId:guid}", GetPost)
            .WithTags("Posts")
            .WithName("GetPost");

        // Delete post (authenticated, owner only)
        app.MapDelete("/api/posts/{postId:guid}", DeletePost)
            .WithTags("Posts")
            .WithName("DeletePost")
            .RequireAuthorization();

        // Like post (authenticated)
        app.MapPost("/api/posts/{postId:guid}/like", LikePost)
            .WithTags("Posts")
            .WithName("LikePost")
            .RequireAuthorization();

        // Unlike post (authenticated)
        app.MapDelete("/api/posts/{postId:guid}/like", UnlikePost)
            .WithTags("Posts")
            .WithName("UnlikePost")
            .RequireAuthorization();

        // Get comments for a post (public)
        app.MapGet("/api/posts/{postId:guid}/comments", GetComments)
            .WithTags("Posts")
            .WithName("GetComments");

        // Create comment (authenticated)
        app.MapPost("/api/posts/{postId:guid}/comments", CreateComment)
            .WithTags("Posts")
            .WithName("CreateComment")
            .RequireAuthorization();

        // Delete comment (authenticated, owner only)
        app.MapDelete("/api/comments/{commentId:guid}", DeleteComment)
            .WithTags("Posts")
            .WithName("DeleteComment")
            .RequireAuthorization();
    }

    private static async Task<IResult> CreatePost(
        [FromBody] CreatePostRequest request,
        ClaimsPrincipal user,
        IPostService postService)
    {
        var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var accountId))
        {
            return Results.Unauthorized();
        }

        try
        {
            var post = await postService.CreatePostAsync(accountId, request.Content);
            if (post == null)
            {
                return Results.BadRequest(new ErrorResponse("Failed to create post"));
            }

            var response = new PostResponse(
                post.PostId,
                post.AuthorAccount?.AccountId ?? Guid.Empty,
                post.AuthorAccount?.Username ?? "Unknown",
                post.AuthorAccount?.Profile?.DisplayName ?? post.AuthorAccount?.Username ?? "Unknown",
                post.AuthorAccount?.Profile?.AvatarUrl,
                post.Content,
                post.CreatedAt,
                post.UpdatedAt,
                0,
                0,
                false
            );

            return Results.Created($"/api/posts/{post.PostId}", response);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ErrorResponse(ex.Message));
        }
    }

    private static async Task<IResult> GetPost(
        Guid postId,
        ClaimsPrincipal? user,
        IPostService postService)
    {
        var post = await postService.GetPostByIdAsync(postId);
        if (post == null)
        {
            return Results.NotFound(new ErrorResponse("Post not found"));
        }

        // Get engagement counts
        var likeCount = await postService.GetLikeCountAsync(postId);
        var commentCount = await postService.GetCommentCountAsync(postId);

        // Check if current user liked the post
        bool isLikedByCurrentUser = false;
        if (user != null && user.Identity?.IsAuthenticated == true)
        {
            var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim != null && int.TryParse(accountIdClaim.Value, out var accountId))
            {
                isLikedByCurrentUser = await postService.IsPostLikedByAccountAsync(accountId, postId);
            }
        }

        var response = new PostResponse(
            post.PostId,
            post.AuthorAccount?.AccountId ?? Guid.Empty,
            post.AuthorAccount?.Username ?? "Unknown",
            post.AuthorAccount?.Profile?.DisplayName ?? post.AuthorAccount?.Username ?? "Unknown",
            post.AuthorAccount?.Profile?.AvatarUrl,
            post.Content,
            post.CreatedAt,
            post.UpdatedAt,
            likeCount,
            commentCount,
            isLikedByCurrentUser
        );

        return Results.Ok(response);
    }

    private static async Task<IResult> DeletePost(
        Guid postId,
        ClaimsPrincipal user,
        IPostService postService)
    {
        var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var accountId))
        {
            return Results.Unauthorized();
        }

        try
        {
            var result = await postService.DeletePostAsync(accountId, postId);
            if (result)
            {
                return Results.Ok(new EngagementActionResponse(true, "Post deleted"));
            }
            return Results.NotFound(new ErrorResponse("Post not found"));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ErrorResponse(ex.Message));
        }
    }

    private static async Task<IResult> LikePost(
        Guid postId,
        ClaimsPrincipal user,
        IPostService postService)
    {
        var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var accountId))
        {
            return Results.Unauthorized();
        }

        try
        {
            var like = await postService.LikePostAsync(accountId, postId);
            if (like != null)
            {
                return Results.Ok(new EngagementActionResponse(true, "Post liked"));
            }
            return Results.BadRequest(new ErrorResponse("Failed to like post"));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ErrorResponse(ex.Message));
        }
    }

    private static async Task<IResult> UnlikePost(
        Guid postId,
        ClaimsPrincipal user,
        IPostService postService)
    {
        var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var accountId))
        {
            return Results.Unauthorized();
        }

        var result = await postService.UnlikePostAsync(accountId, postId);
        if (result)
        {
            return Results.Ok(new EngagementActionResponse(true, "Post unliked"));
        }
        return Results.Ok(new EngagementActionResponse(false, "Like not found"));
    }

    private static async Task<IResult> GetComments(
        Guid postId,
        [FromQuery] int page = 1,
        IPostService postService = null!)
    {
        // Verify post exists
        var post = await postService.GetPostByIdAsync(postId);
        if (post == null)
        {
            return Results.NotFound(new ErrorResponse("Post not found"));
        }

        page = page < 1 ? 1 : page;
        var pageSize = 20;

        var (items, totalCount) = await postService.GetCommentsAsync(postId, page, pageSize);

        var responses = items.Select(c => new CommentResponse(
            c.CommentId,
            postId,
            c.AuthorAccount?.AccountId ?? Guid.Empty,
            c.AuthorAccount?.Username ?? "Unknown",
            c.AuthorAccount?.Profile?.DisplayName ?? c.AuthorAccount?.Username ?? "Unknown",
            c.AuthorAccount?.Profile?.AvatarUrl,
            c.Content,
            c.CreatedAt,
            c.UpdatedAt
        ));

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return Results.Ok(new PaginatedCommentsResponse(responses, page, pageSize, totalCount, totalPages));
    }

    private static async Task<IResult> CreateComment(
        Guid postId,
        [FromBody] CreateCommentRequest request,
        ClaimsPrincipal user,
        IPostService postService)
    {
        var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var accountId))
        {
            return Results.Unauthorized();
        }

        try
        {
            var comment = await postService.CreateCommentAsync(accountId, postId, request.Content);
            if (comment == null)
            {
                return Results.BadRequest(new ErrorResponse("Failed to create comment"));
            }

            var response = new CommentResponse(
                comment.CommentId,
                postId,
                comment.AuthorAccount?.AccountId ?? Guid.Empty,
                comment.AuthorAccount?.Username ?? "Unknown",
                comment.AuthorAccount?.Profile?.DisplayName ?? comment.AuthorAccount?.Username ?? "Unknown",
                comment.AuthorAccount?.Profile?.AvatarUrl,
                comment.Content,
                comment.CreatedAt,
                comment.UpdatedAt
            );

            return Results.Created($"/api/comments/{comment.CommentId}", response);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ErrorResponse(ex.Message));
        }
    }

    private static async Task<IResult> DeleteComment(
        Guid commentId,
        ClaimsPrincipal user,
        IPostService postService)
    {
        var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var accountId))
        {
            return Results.Unauthorized();
        }

        try
        {
            var result = await postService.DeleteCommentAsync(accountId, commentId);
            if (result)
            {
                return Results.Ok(new EngagementActionResponse(true, "Comment deleted"));
            }
            return Results.NotFound(new ErrorResponse("Comment not found"));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ErrorResponse(ex.Message));
        }
    }
}

// Request DTOs
public record CreatePostRequest(string Content);
public record CreateCommentRequest(string Content);
