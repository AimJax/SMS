using SocialMediaSimulator.Client.ViewModels;
using SocialMediaSimulator.Client.Services;
using SocialMediaSimulator.Client.Models;

namespace SocialMediaSimulator.Client.Views;

public partial class SearchPage : ContentPage
{
    private readonly SearchViewModel _viewModel;

    public event EventHandler<int>? AccountSelected;
    public event EventHandler<Post>? PostSelected;

    public SearchPage() : this(App.ApiService) { }

    public SearchPage(ApiService apiService)
    {
        InitializeComponent();
        _viewModel = new SearchViewModel(apiService);
        BindingContext = _viewModel;
        
        _viewModel.AccountSelected += (_, id) => AccountSelected?.Invoke(this, id);
        _viewModel.PostSelected += (_, post) => PostSelected?.Invoke(this, post);
        
        _ = _viewModel.LoadTrendsAsync();
    }
}
