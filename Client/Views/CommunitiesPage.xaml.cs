using SocialMediaSimulator.Client.ViewModels;
using SocialMediaSimulator.Client.Services;
using SocialMediaSimulator.Client.Models;

namespace SocialMediaSimulator.Client.Views;

public partial class CommunitiesPage : ContentPage
{
    private readonly CommunitiesViewModel _viewModel;

    public event EventHandler<Post>? PostSelected;

    public CommunitiesPage() : this(App.ApiService) { }

    public CommunitiesPage(ApiService apiService)
    {
        InitializeComponent();
        _viewModel = new CommunitiesViewModel(apiService);
        BindingContext = _viewModel;
        
        _viewModel.PostTapped += (_, post) => PostSelected?.Invoke(this, post);
        
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
