using SocialMediaSimulator.Client.ViewModels;
using SocialMediaSimulator.Client.Services;
using SocialMediaSimulator.Client.Models;

namespace SocialMediaSimulator.Client.Views;

public partial class FeedPage : ContentPage
{
    private readonly FeedViewModel _viewModel;

    public event EventHandler? CreatePostRequested;
    public event EventHandler<Post>? PostSelected;
    public event EventHandler<int>? AccountSelected;

    public FeedPage() : this(App.ApiService) { }

    public FeedPage(ApiService apiService)
    {
        InitializeComponent();
        _viewModel = new FeedViewModel(apiService);
        BindingContext = _viewModel;
        
        _viewModel.CreatePostRequested += (_, _) => CreatePostRequested?.Invoke(this, EventArgs.Empty);
        _viewModel.PostSelected += (_, post) => PostSelected?.Invoke(this, post);
        _viewModel.AccountSelected += (_, id) => AccountSelected?.Invoke(this, id);
        
        // Load data when page appears
        Appearing += OnAppearing;
    }

    void OnAppearing(object? sender, EventArgs e)
    {
        _ = _viewModel.LoadAsync();
    }

    public void Refresh()
    {
        _ = _viewModel.LoadAsync();
    }
}
