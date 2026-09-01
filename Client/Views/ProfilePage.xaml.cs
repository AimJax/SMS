using SocialMediaSimulator.Client.ViewModels;
using SocialMediaSimulator.Client.Services;
using SocialMediaSimulator.Client.Models;

namespace SocialMediaSimulator.Client.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public event EventHandler? LogoutRequested;
    public event EventHandler<Post>? PostSelected;

    public ProfilePage() : this(App.ApiService) { }

    public ProfilePage(ApiService apiService)
    {
        InitializeComponent();
        _viewModel = new ProfileViewModel(apiService);
        BindingContext = _viewModel;
        
        Appearing += OnAppearing;
    }

    void OnAppearing(object? sender, EventArgs e)
    {
        if (_viewModel.Account == null)
        {
            LoadOwnProfile();
        }
    }

    public void LoadOwnProfile()
    {
        _viewModel.IsOwnProfile = true;
        _ = _viewModel.LoadOwnProfileAsync();
    }

    public void LoadProfile(int accountId)
    {
        _viewModel.IsOwnProfile = false;
        _ = _viewModel.LoadProfileAsync(accountId);
    }

    void OnLogoutClicked(object sender, EventArgs e)
    {
        App.Logout();
    }
}
