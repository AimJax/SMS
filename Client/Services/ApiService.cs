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

    public ApiService(HttpClient httpClient, AppConfig config, ILogger<ApiService>? logger = null)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Checks if the backend server is healthy by calling GET /api/health.
    /// </summary>
    /// <returns>True if server is online and responding, false otherwise.</returns>
    public async Task<(bool IsOnline, string Status, string? Error)> CheckServerHealthAsync()
    {
        try
        {
            var url = $"{_config.ApiBaseUrl.TrimEnd('/')}/api/health";
            _logger?.LogDebug("Checking server health at: {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var healthResponse = JsonSerializer.Deserialize<HealthResponse>(content);
                
                var status = healthResponse?.status ?? "unknown";
                _logger?.LogInformation("Server health check: {Status}", status);
                
                return (status == "ok", status, null);
            }
            else
            {
                var error = $"HTTP {(int)response.StatusCode}";
                _logger?.LogWarning("Server health check failed: {Error}", error);
                return (false, "error", error);
            }
        }
        catch (TaskCanceledException)
        {
            _logger?.LogWarning("Server health check timed out");
            return (false, "offline", "Request timed out");
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogWarning(ex, "Server health check failed: {Message}", ex.Message);
            return (false, "offline", ex.Message);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error during health check");
            return (false, "error", ex.Message);
        }
    }
}
