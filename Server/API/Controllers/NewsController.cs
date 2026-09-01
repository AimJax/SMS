using Microsoft.AspNetCore.Mvc;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.API.Controllers;

public static class NewsControllerExtensions
{
    public static void MapNewsEndpoints(this WebApplication app)
    {
        // News accounts
        app.MapGet("/api/news/accounts", async (INewsService newsService) =>
        {
            var accounts = await newsService.GetNewsAccountsAsync();
            return Results.Ok(accounts);
        });

        app.MapGet("/api/news/accounts/{id:guid}", async (Guid id, INewsService newsService) =>
        {
            var account = await newsService.GetNewsAccountAsync(id);
            if (account == null) return Results.NotFound();
            return Results.Ok(account);
        });

        // News articles
        app.MapGet("/api/news", async (INewsService newsService, int count = 20, string? category = null) =>
        {
            NewsCategory? categoryEnum = null;
            if (!string.IsNullOrEmpty(category) && Enum.TryParse<NewsCategory>(category, true, out var parsed))
            {
                categoryEnum = parsed;
            }
            var articles = await newsService.GetLatestNewsAsync(count, categoryEnum);
            return Results.Ok(articles);
        });

        app.MapGet("/api/news/{id:guid}", async (Guid id, INewsService newsService) =>
        {
            var article = await newsService.GetArticleAsync(id);
            if (article == null) return Results.NotFound();
            return Results.Ok(article);
        });

        app.MapGet("/api/news/breaking", async (INewsService newsService, int count = 10) =>
        {
            var articles = await newsService.GetBreakingNewsAsync(count);
            return Results.Ok(articles);
        });

        // Processing
        app.MapPost("/api/news/process", async (INewsService newsService) =>
        {
            await newsService.ProcessNewsTickAsync();
            return Results.Ok(new { status = "processed" });
        });
    }
}
