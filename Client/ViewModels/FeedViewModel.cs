using System.Collections.ObjectModel;
using System.Windows.Input;
using SocialMediaSimulator.Client.Services;
using SocialMediaSimulator.Client.Models;

namespace SocialMediaSimulator.Client.ViewModels;

public class FeedViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private ObservableCollection<Post> _posts = new();
    private bool _isLoading;
    private bool _isRefreshing;
    private string? _errorMessage;

    public event EventHandler<Post>? PostSelected;
    public event EventHandler? CreatePostRequested;

    public FeedViewModel(ApiService apiService)
    {
        _apiService = apiService;
        RefreshCommand = new Command(async () => await RefreshAsync());
        LoadMoreCommand = new Command(async () => await LoadMoreAsync());
        CreatePostCommand = new Command(() => CreatePostRequested?.Invoke(this, EventArgs.Empty));
        LikePostCommand = new Command<Post>(async (post) => await ToggleLikeAsync(post));
        CommentPostCommand = new Command<Post>((post) => PostSelected?.Invoke(this, post));
        AccountSelectedCommand = new Command<Post>((post) => AccountSelected?.Invoke(this, post.AuthorAccountId));
    }

    public ObservableCollection<Post> Posts
    {
        get => _posts;
        set => SetProperty(ref _posts, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public event EventHandler<int>? AccountSelected;

    public ICommand RefreshCommand { get; }
    public ICommand LoadMoreCommand { get; }
    public ICommand CreatePostCommand { get; }
    public ICommand LikePostCommand { get; }
    public ICommand CommentPostCommand { get; }
    public ICommand AccountSelectedCommand { get; }

    int _offset = 0;
    const int PageSize = 20;

    public async Task LoadAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        ErrorMessage = null;
        _offset = 0;

        try
        {
            var posts = await _apiService.GetHomeFeedAsync(PageSize);
            Posts = new ObservableCollection<Post>(posts ?? new List<Post>());
            _offset = Posts.Count;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadAsync();
        IsRefreshing = false;
    }

    async Task LoadMoreAsync()
    {
        if (IsLoading || Posts.Count == 0) return;
        IsLoading = true;

        try
        {
            var posts = await _apiService.GetHomeFeedAsync(PageSize, _offset);
            if (posts != null)
            {
                foreach (var post in posts)
                    Posts.Add(post);
                _offset += posts.Count;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    async Task ToggleLikeAsync(Post post)
    {
        bool newState;
        if (post.IsLiked)
        {
            await _apiService.UnlikePostAsync(post.PostId);
            newState = false;
            post.LikeCount--;
        }
        else
        {
            await _apiService.LikePostAsync(post.PostId);
            newState = true;
            post.LikeCount++;
        }
        post.IsLiked = newState;
        OnPropertyChanged(nameof(Posts));
    }
}
