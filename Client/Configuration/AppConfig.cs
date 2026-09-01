namespace SocialMediaSimulator.Client.Configuration;

/// <summary>
/// Client configuration for connecting to the backend server.
/// </summary>
public class AppConfig
{
    /// <summary>
    /// Base URL of the API server.
    /// For Android emulator use: http://10.0.2.2:5225 (emulator's host loopback)
    /// For physical device on same network: Use your machine's local IP address.
    /// </summary>
    public string ApiBaseUrl { get; set; } = "http://192.168.1.47:5225";
}
