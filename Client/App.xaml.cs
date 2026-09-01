using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using SocialMediaSimulator.Client.Services;
using SocialMediaSimulator.Client.Views;
using SocialMediaSimulator.Client.Configuration;
using SocialMediaSimulator.Client.Models;

namespace SocialMediaSimulator.Client;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static ApiService ApiService { get; private set; } = null!;
    public static Account? CurrentAccount { get; private set; }
    public static string? AuthToken { get; private set; }

    public App()
    {
        InitializeComponent();
        
        // Configure services
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();
        ApiService = Services.GetRequiredService<ApiService>();
    }

    static void ConfigureServices(IServiceCollection services)
    {
        // Register AppConfig
        services.AddSingleton(new AppConfig());
        
        // Register HttpClient for ApiService
        services.AddHttpClient<ApiService>((sp, client) =>
        {
            var config = sp.GetRequiredService<AppConfig>();
            client.BaseAddress = new Uri(config.ApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Try to restore session
        RestoreSession();

        if (CurrentAccount != null && !string.IsNullOrEmpty(AuthToken))
        {
            // Show main app
            return new Window(new MainShell());
        }
        else
        {
            // Show auth page
            return new Window(new AuthShell());
        }
    }

    public static void SetAuthenticated(Account account, string token)
    {
        CurrentAccount = account;
        AuthToken = token;
        ApiService.SetAuthToken(token);
    }

    public static void SetToken(string token)
    {
        AuthToken = token;
        ApiService.SetAuthToken(token);
    }

    public static void Logout()
    {
        CurrentAccount = null;
        AuthToken = null;
        ApiService.Logout();
        
        // Use Windows[0] to update root page
        if (Current.Windows.Count > 0)
        {
            Current.Windows[0].Page = new AuthShell();
        }
    }

    void RestoreSession()
    {
        // For now, start fresh - session restoration would use secure storage
    }
}
