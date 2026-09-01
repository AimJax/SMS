using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SocialMediaSimulator.Client.Configuration;
using SocialMediaSimulator.Client.Models;

namespace SocialMediaSimulator.Client.Services;

/// <summary>
/// Service for communicating with the backend API.
/// </summary>
public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly AppConfig _config;
    private readonly ILogger<ApiService>? _logger;
    private string? _authToken;
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiService(HttpClient httpClient, AppConfig config, ILogger<ApiService>? logger = null)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Sets the auth token for subsequent requests.
    /// </summary>
    public void SetAuthToken(string? token)
    {
        _authToken = token;
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    private string BaseUrl => _config.ApiBaseUrl.TrimEnd('/');
    
    private async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}{endpoint}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, JsonOptions);
            }
            _logger?.LogWarning("GET {Endpoint} failed: {StatusCode}", endpoint, response.StatusCode);
            return default;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "GET {Endpoint} error", endpoint);
            return default;
        }
    }

    private async Task<T?> PostAsync<T>(string endpoint, object? body = null)
    {
        try
        {
            HttpContent? content = null;
            if (body != null)
            {
                var json = JsonSerializer.Serialize(body, JsonOptions);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            var response = await _httpClient.PostAsync($"{BaseUrl}{endpoint}", content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, JsonOptions);
            }
            _logger?.LogWarning("POST {Endpoint} failed: {StatusCode}", endpoint, response.StatusCode);
            return default;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "POST {Endpoint} error", endpoint);
            return default;
        }
    }

    private async Task<bool> PostNoResponseAsync(string endpoint, object? body = null)
    {
        try
        {
            HttpContent? content = null;
            if (body != null)
            {
                var json = JsonSerializer.Serialize(body, JsonOptions);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            var response = await _httpClient.PostAsync($"{BaseUrl}{endpoint}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "POST {Endpoint} error", endpoint);
            return false;
        }
    }

    #region Health

    public async Task<(bool IsOnline, string Status, string? Error)> CheckServerHealthAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/api/health");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var healthResponse = JsonSerializer.Deserialize<HealthResponse>(content, JsonOptions);
                return (healthResponse?.status == "ok", healthResponse?.status ?? "unknown", null);
            }
            return (false, "error", $"HTTP {(int)response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, "offline", ex.Message);
        }
    }

    #endregion

    #region Auth

    public async Task<AuthResponse?> LoginAsync(string email, string password)
    {
        var request = new { Email = email, Password = password };
        var response = await PostAsync<AuthResponse>("/api/auth/login", request);
        if (response?.Success == true && !string.IsNullOrEmpty(response.Token))
        {
            SetAuthToken(response.Token);
        }
        return response;
    }

    public async Task<AuthResponse?> RegisterAsync(string username, string displayName, string email, string password)
    {
        var request = new RegisterRequest 
        { 
            Username = username, 
            DisplayName = displayName, 
            Email = email, 
            Password = password 
        };
        var response = await PostAsync<AuthResponse>("/api/auth/register", request);
        if (response?.Success == true && !string.IsNullOrEmpty(response.Token))
        {
            SetAuthToken(response.Token);
        }
        return response;
    }

    public void Logout()
    {
        SetAuthToken(null);
    }

    #endregion

    #region Account

    public async Task<Account?> GetCurrentAccountAsync()
    {
        return await GetAsync<Account>("/api/me");
    }

    public async Task<Account?> GetAccountByIdAsync(int accountId)
    {
        return await GetAsync<Account>($"/api/accounts/{accountId}");
    }

    public async Task<Account?> UpdateProfileAsync(string displayName, string? bio, string? avatarUrl)
    {
        var request = new { DisplayName = displayName, Bio = bio, AvatarUrl = avatarUrl };
        return await PostAsync<Account>("/api/me/profile", request);
    }

    #endregion

    #region Feed

    public async Task<List<Post>?> GetHomeFeedAsync(int count = 20, int offset = 0)
    {
        return await GetAsync<List<Post>>($"/api/feed?count={count}&offset={offset}");
    }

    public async Task<List<Post>?> GetUserPostsAsync(int accountId, int count = 20)
    {
        return await GetAsync<List<Post>>($"/api/accounts/{accountId}/posts?count={count}");
    }

    #endregion

    #region Posts

    public async Task<Post?> CreatePostAsync(string content, int? communityId = null, string? topic = null)
    {
        var request = new { Content = content, CommunityId = communityId, Topic = topic };
        return await PostAsync<Post>("/api/posts", request);
    }

    public async Task<Post?> GetPostAsync(Guid postId)
    {
        return await GetAsync<Post>($"/api/posts/{postId}");
    }

    public async Task<bool> LikePostAsync(Guid postId)
    {
        return await PostNoResponseAsync($"/api/posts/{postId}/like");
    }

    public async Task<bool> UnlikePostAsync(Guid postId)
    {
        return await PostNoResponseAsync($"/api/posts/{postId}/unlike");
    }

    public async Task<bool> DeletePostAsync(Guid postId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/api/posts/{postId}");
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    #endregion

    #region Comments

    public async Task<List<Comment>?> GetCommentsAsync(Guid postId)
    {
        return await GetAsync<List<Comment>>($"/api/posts/{postId}/comments");
    }

    public async Task<Comment?> CreateCommentAsync(Guid postId, string content)
    {
        var request = new { Content = content };
        return await PostAsync<Comment>($"/api/posts/{postId}/comments", request);
    }

    public async Task<bool> LikeCommentAsync(Guid commentId)
    {
        return await PostNoResponseAsync($"/api/comments/{commentId}/like");
    }

    #endregion

    #region Social Graph

    public async Task<bool> FollowAccountAsync(int accountId)
    {
        return await PostNoResponseAsync($"/api/accounts/{accountId}/follow");
    }

    public async Task<bool> UnfollowAccountAsync(int accountId)
    {
        return await PostNoResponseAsync($"/api/accounts/{accountId}/unfollow");
    }

    public async Task<List<Account>?> GetFollowersAsync(int accountId)
    {
        return await GetAsync<List<Account>>($"/api/accounts/{accountId}/followers");
    }

    public async Task<List<Account>?> GetFollowingAsync(int accountId)
    {
        return await GetAsync<List<Account>>($"/api/accounts/{accountId}/following");
    }

    #endregion

    #region Notifications

    public async Task<List<Notification>?> GetNotificationsAsync(int count = 20)
    {
        return await GetAsync<List<Notification>>($"/api/notifications?count={count}");
    }

    public async Task<bool> MarkNotificationReadAsync(Guid notificationId)
    {
        return await PostNoResponseAsync($"/api/notifications/{notificationId}/read");
    }

    public async Task<bool> MarkAllNotificationsReadAsync()
    {
        return await PostNoResponseAsync("/api/notifications/read-all");
    }

    public async Task<int?> GetUnreadNotificationCountAsync()
    {
        var response = await GetAsync<UnreadCountResponse>("/api/notifications/unread-count");
        return response?.Count;
    }

    private class UnreadCountResponse { public int Count { get; set; } }

    #endregion

    #region Communities

    public async Task<List<Community>?> GetCommunitiesAsync()
    {
        return await GetAsync<List<Community>>("/api/communities");
    }

    public async Task<Community?> GetCommunityAsync(int communityId)
    {
        return await GetAsync<Community>($"/api/communities/{communityId}");
    }

    public async Task<bool> JoinCommunityAsync(int communityId)
    {
        return await PostNoResponseAsync($"/api/communities/{communityId}/join");
    }

    public async Task<bool> LeaveCommunityAsync(int communityId)
    {
        return await PostNoResponseAsync($"/api/communities/{communityId}/leave");
    }

    public async Task<List<Post>?> GetCommunityFeedAsync(int communityId, int count = 20)
    {
        return await GetAsync<List<Post>>($"/api/communities/{communityId}/feed?count={count}");
    }

    #endregion

    #region Search

    public async Task<List<Account>?> SearchAccountsAsync(string query)
    {
        return await GetAsync<List<Account>>($"/api/search/accounts?q={Uri.EscapeDataString(query)}");
    }

    public async Task<List<Post>?> SearchPostsAsync(string query)
    {
        return await GetAsync<List<Post>>($"/api/search/posts?q={Uri.EscapeDataString(query)}");
    }

    #endregion

    #region Trends

    public async Task<List<Trend>?> GetTrendsAsync()
    {
        return await GetAsync<List<Trend>>("/api/trends");
    }

    #endregion

    #region News

    public async Task<List<NewsArticle>?> GetNewsAsync(int count = 20)
    {
        return await GetAsync<List<NewsArticle>>($"/api/news?count={count}");
    }

    public async Task<List<NewsArticle>?> GetBreakingNewsAsync(int count = 10)
    {
        return await GetAsync<List<NewsArticle>>($"/api/news/breaking?count={count}");
    }

    public async Task<NewsArticle?> GetNewsArticleAsync(Guid articleId)
    {
        return await GetAsync<NewsArticle>($"/api/news/{articleId}");
    }

    #endregion
}
