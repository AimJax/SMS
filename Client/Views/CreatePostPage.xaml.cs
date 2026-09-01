using SocialMediaSimulator.Client.Services;
using SocialMediaSimulator.Client.Models;

namespace SocialMediaSimulator.Client.Views;

public partial class CreatePostPage : ContentPage
{
    private readonly ApiService _apiService;
    private string _content = string.Empty;
    private string _topic = string.Empty;
    private bool _isPosting;

    public new event EventHandler<Post>? PostCreated;

    public CreatePostPage() : this(App.ApiService) { }

    public CreatePostPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    public string PostContent
    {
        get => _content;
        set
        {
            _content = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CharCount));
            OnPropertyChanged(nameof(CanPost));
        }
    }

    public string Topic
    {
        get => _topic;
        set { _topic = value; OnPropertyChanged(); }
    }

    public int CharCount => _content.Length;
    public bool CanPost => !_isPosting && _content.Length > 0 && _content.Length <= 280;
    public bool IsPosting { get => _isPosting; set { _isPosting = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanPost)); } }
    public string? ErrorMessage { get; private set; }

    void OnContentChanged(object? sender, TextChangedEventArgs e)
    {
        PostContent = e.NewTextValue ?? string.Empty;
    }

    async void OnPostClicked(object? sender, EventArgs e)
    {
        if (!CanPost) return;
        IsPosting = true;
        ErrorMessage = null;

        try
        {
            var post = await _apiService.CreatePostAsync(PostContent, null, 
                string.IsNullOrWhiteSpace(Topic) ? null : Topic);
            if (post != null)
            {
                PostCreated?.Invoke(this, post);
                await Navigation.PopAsync();
            }
            else
            {
                ErrorMessage = "Failed to create post";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsPosting = false;
        }
    }
}
