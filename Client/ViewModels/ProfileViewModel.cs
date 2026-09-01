using System.Collections.ObjectModel;
using System.Windows.Input;
using SocialMediaSimulator.Client.Services;
using SocialMediaSimulator.Client.Models;

namespace SocialMediaSimulator.Client.ViewModels;

public class ProfileViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private Account? _account;
    private ObservableCollection<Post> _posts = new();
    private bool _isLoading;
    private bool _isOwnProfile;
    private bool _isFollowing;
    private int _followerCount;
    private int _followingCount;

    public ProfileViewModel(ApiService apiService)
    {
        _apiService = apiService;
        RefreshCommand = new Command(async () => await RefreshAsync());
        FollowCommand = new Command(async () => await ToggleFollowAsync());
        PostSelectedCommand = new Command<Post>((post) => PostSelected?.Invoke(this, post));
    }

    public event EventHandler<Post>? PostSelected;

    public Account? Account
    {
        get => _account;
        set => SetProperty(ref _account, value);
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

    public bool IsOwnProfile
    {
        get => _isOwnProfile;
        set => SetProperty(ref _isOwnProfile, value);
    }

    public bool IsFollowing
    {
        get => _isFollowing;
        set => SetProperty(ref _isFollowing, value);
    }

    public int FollowerCount
    {
        get => _followerCount;
        set => SetProperty(ref _followerCount, value);
    }

    public int FollowingCount
    {
        get => _followingCount;
        set => SetProperty(ref _followingCount, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand FollowCommand { get; }
    public ICommand PostSelectedCommand { get; }

    public async Task LoadOwnProfileAsync()
    {
        IsOwnProfile = true;
        IsLoading = true;

        try
        {
            Account = await _apiService.GetCurrentAccountAsync();
            if (Account != null)
            {
                var posts = await _apiService.GetUserPostsAsync(Account.Id);
                Posts = new ObservableCollection<Post>(posts ?? new List<Post>());
                FollowerCount = Account.Profile?.FollowerCount ?? 0;
                FollowingCount = Account.Profile?.FollowingCount ?? 0;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    async Task RefreshAsync()
    {
        if (IsOwnProfile)
            await LoadOwnProfileAsync();
        else if (Account != null)
            await LoadProfileAsync(Account.Id);
    }

    public async Task LoadProfileAsync(int accountId)
    {
        IsOwnProfile = false;
        IsLoading = true;

        try
        {
            Account = await _apiService.GetAccountByIdAsync(accountId);
            if (Account != null)
            {
                var posts = await _apiService.GetUserPostsAsync(accountId);
                Posts = new ObservableCollection<Post>(posts ?? new List<Post>());
                FollowerCount = Account.Profile?.FollowerCount ?? 0;
                FollowingCount = Account.Profile?.FollowingCount ?? 0;
                IsFollowing = false; // TODO: Check if following
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    async Task ToggleFollowAsync()
    {
        if (Account == null || IsOwnProfile) return;

        if (IsFollowing)
        {
            await _apiService.UnfollowAccountAsync(Account.Id);
            IsFollowing = false;
            FollowerCount--;
        }
        else
        {
            await _apiService.FollowAccountAsync(Account.Id);
            IsFollowing = true;
            FollowerCount++;
        }
    }
}
