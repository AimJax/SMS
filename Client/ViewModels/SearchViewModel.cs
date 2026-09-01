using System.Collections.ObjectModel;
using System.Windows.Input;
using SocialMediaSimulator.Client.Services;
using SocialMediaSimulator.Client.Models;

namespace SocialMediaSimulator.Client.ViewModels;

public class SearchViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private string _searchQuery = string.Empty;
    private ObservableCollection<Account> _accounts = new();
    private ObservableCollection<Post> _posts = new();
    private ObservableCollection<Trend> _trends = new();
    private bool _isSearching;
    private bool _hasSearched;

    public event EventHandler<int>? AccountSelected;
    public event EventHandler<Post>? PostSelected;

    public SearchViewModel(ApiService apiService)
    {
        _apiService = apiService;
        SearchCommand = new Command(async () => await SearchAsync());
        AccountTappedCommand = new Command<Account>((a) => AccountSelected?.Invoke(this, a.Id));
        PostTappedCommand = new Command<Post>((p) => PostSelected?.Invoke(this, p));
        LoadTrendsCommand = new Command(async () => await LoadTrendsAsync());
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    public ObservableCollection<Account> Accounts
    {
        get => _accounts;
        set => SetProperty(ref _accounts, value);
    }

    public ObservableCollection<Post> Posts
    {
        get => _posts;
        set => SetProperty(ref _posts, value);
    }

    public ObservableCollection<Trend> Trends
    {
        get => _trends;
        set => SetProperty(ref _trends, value);
    }

    public bool IsSearching
    {
        get => _isSearching;
        set => SetProperty(ref _isSearching, value);
    }

    public bool HasSearched
    {
        get => _hasSearched;
        set => SetProperty(ref _hasSearched, value);
    }

    public ICommand SearchCommand { get; }
    public ICommand AccountTappedCommand { get; }
    public ICommand PostTappedCommand { get; }
    public ICommand LoadTrendsCommand { get; }

    public async Task LoadTrendsAsync()
    {
        var trends = await _apiService.GetTrendsAsync();
        Trends = new ObservableCollection<Trend>(trends ?? new List<Trend>());
    }

    async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;

        IsSearching = true;
        HasSearched = true;

        try
        {
            var accountsTask = _apiService.SearchAccountsAsync(SearchQuery);
            var postsTask = _apiService.SearchPostsAsync(SearchQuery);

            await Task.WhenAll(accountsTask, postsTask);

            Accounts = new ObservableCollection<Account>(accountsTask.Result ?? new List<Account>());
            Posts = new ObservableCollection<Post>(postsTask.Result ?? new List<Post>());
        }
        finally
        {
            IsSearching = false;
        }
    }
}
