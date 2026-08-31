using Microsoft.Extensions.Logging;
using SocialMediaSimulator.Client.Configuration;
using SocialMediaSimulator.Client.Services;

namespace SocialMediaSimulator.Client;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Register client services
        builder.Services.AddSingleton<AppConfig>();
        builder.Services.AddHttpClient<ApiService>();

        return builder.Build();
    }
}
