using SocialMediaSimulator.Client.Views;
using SocialMediaSimulator.Client.Models;

namespace SocialMediaSimulator.Client.Views;

public partial class MainShell : Shell
{
    private FeedPage? _feedPage;
    private ProfilePage? _profilePage;
    private NotificationsPage? _notificationsPage;
    private CommunitiesPage? _communitiesPage;

    public MainShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(CreatePostPage), typeof(CreatePostPage));
        
        // Wire up navigation events
        Loaded += OnLoaded;
    }

    void OnLoaded(object? sender, EventArgs e)
    {
        // Get or create pages
        var shellContent = CurrentItem?.CurrentItem;
        if (shellContent?.Route == "Home")
        {
            SetupFeedPage();
        }
    }

    async void SetupFeedPage()
    {
        if (_feedPage != null) return;
        
        // Navigate to feed tab first
        await GoToAsync("//Home");
        
        // Create and configure feed page
        _feedPage = new FeedPage();
        _feedPage.CreatePostRequested += async (_, _) =>
        {
            await Navigation.PushAsync(new CreatePostPage());
        };
        
        // Also load the profile page
        _profilePage = new ProfilePage();
        _profilePage.LoadOwnProfile();
    }

    protected override void OnNavigating(ShellNavigatingEventArgs args)
    {
        base.OnNavigating(args);
        
        // Handle tab changes
        if (args.Source == ShellNavigationSource.ShellItemChanged)
        {
            var route = CurrentItem?.CurrentItem?.Route;
            switch (route)
            {
                case "Home":
                    if (_feedPage == null) SetupFeedPage();
                    break;
                case "Profile":
                    _profilePage?.LoadOwnProfile();
                    break;
                case "Notifications":
                    _notificationsPage?.Refresh();
                    break;
                case "Communities":
                    _communitiesPage?.Refresh();
                    break;
            }
        }
    }
}
