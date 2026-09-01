using System.Collections.ObjectModel;
using System.Windows.Input;
using SocialMediaSimulator.Client.Services;
using SocialMediaSimulator.Client.Models;

namespace SocialMediaSimulator.Client.ViewModels;

public class CommunitiesViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private ObservableCollection<Community> _communities = new();
    private Community? _selectedCommunity;
    private ObservableCollection<Post> _communityPosts = new();
    private bool _isLoading;
    private bool _isLoadingPosts;

    public event EventHandler<int>? CommunitySelected;

    public CommunitiesViewModel(ApiService apiService)
    {
        _apiService = apiService;
        RefreshCommand = new Command(async () => await LoadAsync());
        SelectCommunityCommand = new Command<Community>(async (c) => await SelectCommunityAsync(c));
        JoinLeaveCommand = new Command<Community>(async (c) => await ToggleJoinAsync(c));
        PostTappedCommand = new Command<Post>((p) => PostTapped?.Invoke(this, p));
    }

    public event EventHandler<Post>? PostTapped;

    public ObservableCollection<Community> Communities
    {
        get => _communities;
        set => SetProperty(ref _communities, value);
    }

    public Community? SelectedCommunity
    {
        get => _selectedCommunity;
        set => SetProperty(ref _selectedCommunity, value);
    }

    public ObservableCollection<Post> CommunityPosts
    {
        get => _communityPosts;
        set => SetProperty(ref _communityPosts, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsLoadingPosts
    {
        get => _isLoadingPosts;
        set => SetProperty(ref _isLoadingPosts, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand SelectCommunityCommand { get; }
    public ICommand JoinLeaveCommand { get; }
    public ICommand PostTappedCommand { get; }

    public async Task LoadAsync()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            var communities = await _apiService.GetCommunitiesAsync();
            Communities = new ObservableCollection<Community>(communities ?? new List<Community>());
        }
        finally
        {
            IsLoading = false;
        }
    }

    async Task SelectCommunityAsync(Community community)
    {
        SelectedCommunity = community;
        IsLoadingPosts = true;

        try
        {
            var posts = await _apiService.GetCommunityFeedAsync(community.Id);
            CommunityPosts = new ObservableCollection<Post>(posts ?? new List<Post>());
        }
        finally
        {
            IsLoadingPosts = false;
        }
    }

    async Task ToggleJoinAsync(Community community)
    {
        if (community.IsJoined)
        {
            await _apiService.LeaveCommunityAsync(community.Id);
            community.IsJoined = false;
            community.MemberCount--;
        }
        else
        {
            await _apiService.JoinCommunityAsync(community.Id);
            community.IsJoined = true;
            community.MemberCount++;
        }
        OnPropertyChanged(nameof(Communities));
    }
}
