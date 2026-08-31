using Microsoft.Extensions.DependencyInjection;
using SocialMediaSimulator.Client.Services;

namespace SocialMediaSimulator.Client;

public partial class MainPage : ContentPage
{
    private readonly ApiService _apiService;

    public MainPage()
    {
        InitializeComponent();

        _apiService = App.Current!.Handler!.MauiContext!.Services.GetRequiredService<ApiService>();
    }

    private async void OnCheckServerClicked(object? sender, EventArgs e)
    {
        CheckButton.IsEnabled = false;
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;
        ErrorLabel.IsVisible = false;
        ServerStatusLabel.Text = "Checking...";
        ServerStatusLabel.TextColor = Colors.Gray;

        var (isOnline, status, error) = await _apiService.CheckServerHealthAsync();

        if (isOnline)
        {
            ServerStatusLabel.Text = "ONLINE";
            ServerStatusLabel.TextColor = Colors.Green;
            ErrorLabel.IsVisible = false;
        }
        else
        {
            ServerStatusLabel.Text = "OFFLINE";
            ServerStatusLabel.TextColor = Colors.Red;
            ErrorLabel.Text = error ?? "Unable to connect to server";
            ErrorLabel.IsVisible = true;
        }

        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;
        CheckButton.IsEnabled = true;
    }
}
