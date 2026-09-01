using SocialMediaSimulator.Client.ViewModels;
using SocialMediaSimulator.Client.Services;
using SocialMediaSimulator.Client.Models;

namespace SocialMediaSimulator.Client.Views;

public partial class NotificationsPage : ContentPage
{
    private readonly NotificationsViewModel _viewModel;

    public event EventHandler<int>? AccountSelected;
    public event EventHandler<Guid>? PostSelected;

    public NotificationsPage() : this(App.ApiService) { }

    public NotificationsPage(ApiService apiService)
    {
        InitializeComponent();
        _viewModel = new NotificationsViewModel(apiService);
        BindingContext = _viewModel;
        
        _viewModel.AccountSelected += (_, id) => AccountSelected?.Invoke(this, id);
        _viewModel.PostSelected += (_, id) => PostSelected?.Invoke(this, id);
        
        Appearing += OnAppearing;
    }

    void OnAppearing(object? sender, EventArgs e)
    {
        _viewModel.LoadAsync();
    }

    public void Refresh()
    {
        _ = _viewModel.LoadAsync();
    }
}
