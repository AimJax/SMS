# SOCIAL MEDIA SIMULATOR — PART 24 DEVELOPMENT PROMPT
## ANDROID UI IMPLEMENTATION — Make the App Usable

You are continuing development of the **Social Media Simulator** from the existing project.

**DO NOT restart, redesign, or replace the existing architecture.**

You must inspect the current repository first and build directly on everything already implemented.

---

# CURRENT PROJECT CHECKPOINT

Completed:

```text
01A  Development Environment         COMPLETE
01B  Repository Foundation           COMPLETE
01C  ASP.NET Core Server            COMPLETE
01D  SQLite Foundation              COMPLETE
01E  Android Client Foundation      COMPLETE
01F  Foundation Checkpoint           COMPLETE
02   Backend Architecture           COMPLETE
03   Persistence                     COMPLETE
04   Accounts & Authentication      COMPLETE
05   Social Graph                    COMPLETE
06   Posts & Engagement              COMPLETE
07   Feed & Timeline                 COMPLETE
08   NPC Simulator Foundation       COMPLETE
09   NPC Population Generation       COMPLETE
10   NPC Behavior Simulation         COMPLETE
11   NPC Background Simulation       COMPLETE
12   NPC Social Graph                COMPLETE
13   AI Content Generation           COMPLETE
14   Notifications System            COMPLETE
15   Communities                     COMPLETE
16   Advanced Feed                   COMPLETE
17   LLM-Driven Event System        COMPLETE
18   Event Causality & Offline Sim   COMPLETE
19   Virality                        COMPLETE
20   Topics & Trends                 COMPLETE
21   Rumors & Misinformation         COMPLETE
22   Deployment & Testing            COMPLETE
23   News                            COMPLETE
```

Latest commit:

```text
87f925f — Part 23: News
```

Remote:

```text
origin/main
```

Repository:

```text
https://github.com/AimJax/SMS.git
```

---

# ⚠️ IMPORTANT: THIS PART IS ANDROID UI ONLY

This part is **NOT** about adding backend features. It's about **building the Android UI** so users can actually interact with the simulation.

The backend is complete. This part adds:
- Authentication screens (login/register)
- Main navigation
- Feed page
- Post creation
- Profile page
- Notifications
- Communities view

---

# 1. WHY THIS PART, NOW

The backend (Parts 01-23) is complete with:
- Accounts, Posts, Feed, Communities
- NPCs with autonomous behavior
- Events, Trends, Virality
- Rumors, News

But the Android app only has a **"Check Server" button**.

Part 24 makes the app **usable** by implementing the UI.

---

# 2. THE EXISTING PROJECT

### Backend (Complete):
- All API endpoints ready
- Authentication with JWT
- Posts, Comments, Likes
- Feed generation
- Notifications
- Communities
- Account profiles
- Search
- Trends
- News articles

### Android (Minimal):
- `MainPage.xaml` — Only has health check button
- `AppShell.xaml` — Basic shell with single page
- `ApiService.cs` — Basic health check
- `AppConfig.cs` — Server URL configuration

### Existing Android Structure:
```
Client/
├── Configuration/
│   └── AppConfig.cs           ← Server URL
├── Services/
│   └── ApiService.cs          ← Health check only
├── Models/
│   └── HealthResponse.cs
├── App.xaml / App.xaml.cs
├── AppShell.xaml / AppShell.xaml.cs
├── MainPage.xaml / MainPage.xaml.cs  ← Only health check
└── Platforms/Android/
    └── AndroidManifest.xml    ← needs usesCleartextTraffic
```

---

# PART 24 — REQUIRED TASKS

## 1. Create Android Models

First, create models that match the server API responses:

### 1.1 Account Model
```csharp
namespace SocialMediaSimulator.Client.Models;

public class Account
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string DisplayName { get; set; }
    public string Bio { get; set; }
    public string ProfileImageUrl { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public int PostCount { get; set; }
    public bool IsFollowing { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### 1.2 Post Model
```csharp
public class Post
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorUsername { get; set; }
    public string AuthorDisplayName { get; set; }
    public string Content { get; set; }
    public int Likes { get; set; }
    public int Comments { get; set; }
    public int Reposts { get; set; }
    public bool IsLiked { get; set; }
    public bool IsReposted { get; set; }
    public List<string> Topics { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### 1.3 Comment Model
```csharp
public class Comment
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorUsername { get; set; }
    public string AuthorDisplayName { get; set; }
    public string Content { get; set; }
    public int Likes { get; set; }
    public bool IsLiked { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### 1.4 Notification Model
```csharp
public class Notification
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public Guid? FromAccountId { get; set; }
    public string? FromUsername { get; set; }
    public Guid? RelatedPostId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### 1.5 Community Model
```csharp
public class Community
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string IconUrl { get; set; }
    public int MemberCount { get; set; }
    public int PostCount { get; set; }
    public bool IsJoined { get; set; }
    public List<string> Topics { get; set; }
}
```

### 1.6 News Article Model
```csharp
public class NewsArticle
{
    public Guid Id { get; set; }
    public Guid NewsAccountId { get; set; }
    public string NewsName { get; set; }
    public string Headline { get; set; }
    public string Summary { get; set; }
    public string Body { get; set; }
    public List<string> Tags { get; set; }
    public int Views { get; set; }
    public bool IsBreakingNews { get; set; }
    public DateTime PublishedAt { get; set; }
}
```

---

## 2. Expand ApiService

Extend the existing ApiService with all necessary API calls:

```csharp
public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly AppConfig _config;
    private string? _authToken;
    private Guid? _currentUserId;

    // === AUTH ===
    public async Task<(bool Success, string Token, Guid UserId)> RegisterAsync(string username, string email, string password, string displayName)
    {
        var response = await PostAsync<AuthResponse>("/api/auth/register", new
        {
            username, email, password, displayName
        });
        
        if (response.Success)
        {
            _authToken = response.Token;
            _currentUserId = response.UserId;
            SaveAuthToken(response.Token);
        }
        
        return (response.Success, response.Token ?? "", response.UserId);
    }

    public async Task<(bool Success, string Token, Guid UserId)> LoginAsync(string email, string password)
    {
        var response = await PostAsync<AuthResponse>("/api/auth/login", new { email, password });
        
        if (response.Success)
        {
            _authToken = response.Token;
            _currentUserId = response.UserId;
            SaveAuthToken(response.Token);
        }
        
        return (response.Success, response.Token ?? "", response.UserId);
    }

    public void Logout()
    {
        _authToken = null;
        _currentUserId = null;
        ClearAuthToken();
    }

    // === FEED ===
    public async Task<List<Post>> GetFeedAsync(int count = 20, string? cursor = null)
    {
        var url = $"/api/feed?count={count}";
        if (cursor != null) url += $"&cursor={cursor}";
        return await GetAsync<List<Post>>(url) ?? new List<Post>();
    }

    // === POSTS ===
    public async Task<Post> CreatePostAsync(string content)
    {
        return await PostAsync<Post>("/api/posts", new { content });
    }

    public async Task<Post> LikePostAsync(Guid postId)
    {
        return await PostAsync<Post>($"/api/posts/{postId}/like", null);
    }

    public async Task<Post> UnlikePostAsync(Guid postId)
    {
        return await DeleteAsync<Post>($"/api/posts/{postId}/like");
    }

    public async Task<Comment> AddCommentAsync(Guid postId, string content)
    {
        return await PostAsync<Comment>($"/api/posts/{postId}/comments", new { content });
    }

    // === ACCOUNTS ===
    public async Task<Account> GetAccountAsync(Guid accountId)
    {
        return await GetAsync<Account>($"/api/accounts/{accountId}");
    }

    public async Task<Account> GetCurrentUserAsync()
    {
        return await GetAsync<Account>("/api/accounts/me");
    }

    public async Task<List<Post>> GetAccountPostsAsync(Guid accountId, int count = 20)
    {
        return await GetAsync<List<Post>>($"/api/accounts/{accountId}/posts?count={count}") ?? new List<Post>();
    }

    public async Task FollowAsync(Guid accountId)
    {
        await PostAsync<object>($"/api/accounts/{accountId}/follow", null);
    }

    public async Task UnfollowAsync(Guid accountId)
    {
        await DeleteAsync<object>($"/api/accounts/{accountId}/follow");
    }

    // === NOTIFICATIONS ===
    public async Task<List<Notification>> GetNotificationsAsync(int count = 20)
    {
        return await GetAsync<List<Notification>>($"/api/notifications?count={count}") ?? new List<Notification>();
    }

    public async Task MarkNotificationReadAsync(Guid notificationId)
    {
        await PostAsync<object>($"/api/notifications/{notificationId}/read", null);
    }

    // === COMMUNITIES ===
    public async Task<List<Community>> GetCommunitiesAsync()
    {
        return await GetAsync<List<Community>>("/api/communities") ?? new List<Community>();
    }

    public async Task<Community> GetCommunityAsync(Guid communityId)
    {
        return await GetAsync<Community>($"/api/communities/{communityId}");
    }

    public async Task JoinCommunityAsync(Guid communityId)
    {
        await PostAsync<object>($"/api/communities/{communityId}/join", null);
    }

    public async Task LeaveCommunityAsync(Guid communityId)
    {
        await DeleteAsync<object>($"/api/communities/{communityId}/join");
    }

    // === NEWS ===
    public async Task<List<NewsArticle>> GetNewsAsync(int count = 20)
    {
        return await GetAsync<List<NewsArticle>>($"/api/news?count={count}") ?? new List<NewsArticle>();
    }

    public async Task<List<NewsArticle>> GetBreakingNewsAsync()
    {
        return await GetAsync<List<NewsArticle>>("/api/news/breaking") ?? new List<NewsArticle>();
    }

    // === TRENDS ===
    public async Task<List<Trend>> GetTrendsAsync(int count = 10)
    {
        return await GetAsync<List<Trend>>($"/api/trends?count={count}") ?? new List<Trend>();
    }

    // === HELPER METHODS ===
    private async Task<T> GetAsync<T>(string endpoint) where T : class
    {
        var response = await _httpClient.GetAsync($"{_config.ApiBaseUrl}{endpoint}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }
        return default;
    }

    private async Task<T> PostAsync<T>(string endpoint, object? body) where T : class
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_config.ApiBaseUrl}{endpoint}");
        if (_authToken != null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        if (body != null)
            request.Content = JsonContent.Create(body);

        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }
        return default;
    }

    private async Task<T> DeleteAsync<T>(string endpoint) where T : class
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{_config.ApiBaseUrl}{endpoint}");
        if (_authToken != null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }
        return default;
    }
}
```

---

## 3. Create Authentication Screens

### 3.1 Login Page (`LoginPage.xaml`)
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             x:Class="SocialMediaSimulator.Client.Pages.LoginPage"
             Title="Login">
    
    <VerticalStackLayout Padding="20" VerticalOptions="Center">
        
        <Label Text="Social Media Simulator" FontSize="28" FontAttributes="Bold" 
               HorizontalOptions="Center" Margin="0,0,0,40"/>
        
        <Entry x:Name="EmailEntry" Placeholder="Email" Keyboard="Email" Margin="0,0,0,10"/>
        <Entry x:Name="PasswordEntry" Placeholder="Password" IsPassword="True" Margin="0,0,0,20"/>
        
        <Button Text="Login" Clicked="OnLoginClicked" Margin="0,0,0,10"/>
        <Button Text="Register" Clicked="OnRegisterClicked" Style="{StaticResource SecondaryButton}"/>
        
        <Label x:Name="ErrorLabel" TextColor="Red" IsVisible="False" Margin="0,20,0,0"/>
        
    </VerticalStackLayout>
</ContentPage>
```

### 3.2 Register Page (`RegisterPage.xaml`)
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             x:Class="SocialMediaSimulator.Client.Pages.RegisterPage"
             Title="Register">
    
    <ScrollView>
        <VerticalStackLayout Padding="20">
            
            <Label Text="Create Account" FontSize="28" FontAttributes="Bold" 
                   HorizontalOptions="Center" Margin="0,20,0,30"/>
            
            <Label Text="Username" FontAttributes="Bold"/>
            <Entry x:Name="UsernameEntry" Placeholder="username" Margin="0,0,0,15"/>
            
            <Label Text="Display Name" FontAttributes="Bold"/>
            <Entry x:Name="DisplayNameEntry" Placeholder="Your name" Margin="0,0,0,15"/>
            
            <Label Text="Email" FontAttributes="Bold"/>
            <Entry x:Name="EmailEntry" Placeholder="email@example.com" Keyboard="Email" Margin="0,0,0,15"/>
            
            <Label Text="Password" FontAttributes="Bold"/>
            <Entry x:Name="PasswordEntry" Placeholder="Password" IsPassword="True" Margin="0,0,0,20"/>
            
            <Button Text="Create Account" Clicked="OnRegisterClicked"/>
            
            <Label x:Name="ErrorLabel" TextColor="Red" IsVisible="False" Margin="0,20,0,0"/>
            
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

---

## 4. Create Main Navigation Shell

### 4.1 Update AppShell.xaml
```xml
<?xml version="1.0" encoding="UTF-8" ?>
<Shell xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
       xmlns:pages="clr-namespace:SocialMediaSimulator.Client.Pages"
       Title="Social Media Simulator">

    <!-- Tab Bar for Main Navigation -->
    <TabBar>
        <Tab Title="Home" Icon="home.png">
            <ShellContent ContentTemplate="{DataTemplate pages:FeedPage}"/>
        </Tab>
        <Tab Title="Search" Icon="search.png">
            <ShellContent ContentTemplate="{DataTemplate pages:SearchPage}"/>
        </Tab>
        <Tab Title="Communities" Icon="community.png">
            <ShellContent ContentTemplate="{DataTemplate pages:CommunitiesPage}"/>
        </Tab>
        <Tab Title="Notifications" Icon="bell.png">
            <ShellContent ContentTemplate="{DataTemplate pages:NotificationsPage}"/>
        </Tab>
        <Tab Title="Profile" Icon="user.png">
            <ShellContent ContentTemplate="{DataTemplate pages:ProfilePage}"/>
        </Tab>
    </TabBar>

</Shell>
```

---

## 5. Create Feed Page

### 5.1 FeedPage.xaml
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             x:Class="SocialMediaSimulator.Client.Pages.FeedPage"
             Title="Feed"
             Appearing="OnAppearing">
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- Feed List -->
        <RefreshView Grid.Row="0" x:Name="RefreshView" Refreshing="OnRefresh">
            <CollectionView x:Name="FeedList" SelectionMode="None">
                <CollectionView.ItemTemplate>
                    <DataTemplate>
                        <Frame Margin="10" Padding="15" CornerRadius="10">
                            <VerticalStackLayout>
                                <!-- Author Header -->
                                <HorizontalStackLayout Margin="0,0,0,10">
                                    <Frame WidthRequest="40" HeightRequest="40" CornerRadius="20" 
                                           BackgroundColor="LightGray" Padding="0">
                                        <Label Text="{Binding AuthorDisplayName[0]}" 
                                               FontSize="20" HorizontalOptions="Center" 
                                               VerticalOptions="Center"/>
                                    </Frame>
                                    <VerticalStackLayout Margin="10,0,0,0">
                                        <Label Text="{Binding AuthorDisplayName}" FontAttributes="Bold"/>
                                        <Label Text="{Binding AuthorUsername}" TextColor="Gray" FontSize="12"/>
                                    </VerticalStackLayout>
                                </HorizontalStackLayout>
                                
                                <!-- Content -->
                                <Label Text="{Binding Content}" FontSize="14" Margin="0,0,0,10"/>
                                
                                <!-- Topics -->
                                <HorizontalStackLayout BindableLayout.ItemsSource="{Binding Topics}" 
                                                       Margin="0,0,0,10">
                                    <BindableLayout.ItemTemplate>
                                        <DataTemplate>
                                            <Frame BackgroundColor="LightBlue" CornerRadius="10" Padding="8,4" Margin="0,0,5,0">
                                                <Label Text="{Binding StringFormat='#{0}'}" FontSize="12"/>
                                            </Frame>
                                        </DataTemplate>
                                    </BindableLayout.ItemTemplate>
                                </HorizontalStackLayout>
                                
                                <!-- Engagement -->
                                <HorizontalStackLayout Spacing="20">
                                    <Button Text="{Binding Likes, StringFormat='♥ {0}'}" 
                                            Style="{StaticResource GhostButton}"
                                            Clicked="OnLikeClicked" CommandParameter="{Binding}"/>
                                    <Button Text="{Binding Comments, StringFormat='💬 {0}'}" 
                                            Style="{StaticResource GhostButton}"
                                            Clicked="OnCommentClicked" CommandParameter="{Binding}"/>
                                    <Button Text="{Binding Reposts, StringFormat='🔁 {0}'}" 
                                            Style="{StaticResource GhostButton}"
                                            Clicked="OnRepostClicked" CommandParameter="{Binding}"/>
                                </HorizontalStackLayout>
                                
                                <!-- Timestamp -->
                                <Label Text="{Binding CreatedAt, StringFormat='{}{0:MMM dd, HH:mm}'}" 
                                       TextColor="Gray" FontSize="11" Margin="0,10,0,0"/>
                            </VerticalStackLayout>
                        </Frame>
                    </DataTemplate>
                </CollectionView.ItemTemplate>
            </CollectionView>
        </RefreshView>
        
        <!-- Create Post FAB -->
        <Button Grid.Row="1" Text="+ Create Post" Clicked="OnCreatePostClicked"
                Margin="20" FontSize="16"/>
        
    </Grid>
</ContentPage>
```

### 5.2 FeedPage.xaml.cs
```csharp
public partial class FeedPage : ContentPage
{
    private readonly ApiService _api;
    private readonly IServiceProvider _services;

    public FeedPage(ApiService api, IServiceProvider services)
    {
        InitializeComponent();
        _api = api;
        _services = services;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadFeedAsync();
    }

    private async Task LoadFeedAsync()
    {
        var posts = await _api.GetFeedAsync(20);
        FeedList.ItemsSource = posts;
    }

    private async void OnRefresh(object sender, EventArgs e)
    {
        await LoadFeedAsync();
        RefreshView.IsRefreshing = false;
    }

    private async void OnLikeClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Post post)
        {
            if (post.IsLiked)
                await _api.UnlikePostAsync(post.Id);
            else
                await _api.LikePostAsync(post.Id);
            
            await LoadFeedAsync();
        }
    }

    private void OnCommentClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Post post)
        {
            // Navigate to comments page
            Navigation.PushAsync(new CommentsPage(post, _api));
        }
    }

    private void OnRepostClicked(object sender, EventArgs e)
    {
        // Repost functionality
    }

    private async void OnCreatePostClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CreatePostPage(_api, async () => await LoadFeedAsync()));
    }
}
```

---

## 6. Create Post Creation Page

### 6.1 CreatePostPage.xaml
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             x:Class="SocialMediaSimulator.Client.Pages.CreatePostPage"
             Title="Create Post">
    
    <Grid Padding="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <Editor x:Name="ContentEditor" Placeholder="What's happening?" 
                FontSize="16" Grid.Row="0"/>
        
        <Button Grid.Row="1" Text="Post" Clicked="OnPostClicked" 
                Margin="0,20,0,0"/>
        
    </Grid>
</ContentPage>
```

### 6.2 CreatePostPage.xaml.cs
```csharp
public partial class CreatePostPage : ContentPage
{
    private readonly ApiService _api;
    private readonly Action _onPosted;

    public CreatePostPage(ApiService api, Action onPosted)
    {
        InitializeComponent();
        _api = api;
        _onPosted = onPosted;
    }

    private async void OnPostClicked(object sender, EventArgs e)
    {
        var content = ContentEditor.Text?.Trim();
        if (string.IsNullOrEmpty(content)) return;

        var post = await _api.CreatePostAsync(content);
        if (post != null)
        {
            _onPosted?.Invoke();
            await Navigation.PopAsync();
        }
    }
}
```

---

## 7. Create Profile Page

### 7.1 ProfilePage.xaml
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             x:Class="SocialMediaSimulator.Client.Pages.ProfilePage"
             Title="Profile"
             Appearing="OnAppearing">
    
    <ScrollView>
        <VerticalStackLayout>
            
            <!-- Profile Header -->
            <Frame HeightRequest="200" BackgroundColor="LightBlue" Padding="0">
                <VerticalStackLayout VerticalOptions="End" Margin="20">
                    <Frame WidthRequest="80" HeightRequest="80" CornerRadius="40" 
                           BackgroundColor="White" HorizontalOptions="Start" Padding="0">
                        <Label x:Name="AvatarLabel" Text="?" FontSize="36" 
                               HorizontalOptions="Center" VerticalOptions="Center"/>
                    </Frame>
                    <Label x:Name="DisplayNameLabel" Text="Loading..." FontSize="24" 
                           FontAttributes="Bold" TextColor="White"/>
                    <Label x:Name="UsernameLabel" Text="@" TextColor="White"/>
                </VerticalStackLayout>
            </Frame>
            
            <!-- Stats -->
            <HorizontalStackLayout Spacing="30" Margin="20" HorizontalOptions="Center">
                <VerticalStackLayout>
                    <Label x:Name="PostsCount" Text="0" FontSize="20" FontAttributes="Bold" 
                           HorizontalOptions="Center"/>
                    <Label Text="Posts" TextColor="Gray" HorizontalOptions="Center"/>
                </VerticalStackLayout>
                <VerticalStackLayout>
                    <Label x:Name="FollowersCount" Text="0" FontSize="20" FontAttributes="Bold" 
                           HorizontalOptions="Center"/>
                    <Label Text="Followers" TextColor="Gray" HorizontalOptions="Center"/>
                </VerticalStackLayout>
                <VerticalStackLayout>
                    <Label x:Name="FollowingCount" Text="0" FontSize="20" FontAttributes="Bold" 
                           HorizontalOptions="Center"/>
                    <Label Text="Following" TextColor="Gray" HorizontalOptions="Center"/>
                </VerticalStackLayout>
            </HorizontalStackLayout>
            
            <!-- Bio -->
            <Label x:Name="BioLabel" Text="" Margin="20,0,20,20"/>
            
            <!-- Logout Button -->
            <Button Text="Logout" Clicked="OnLogoutClicked" 
                    Style="{StaticResource SecondaryButton}" Margin="20,0"/>
            
            <!-- User Posts -->
            <Label Text="My Posts" FontSize="18" FontAttributes="Bold" Margin="20,20,20,10"/>
            <CollectionView x:Name="UserPostsList" HeightRequest="400">
                <CollectionView.ItemTemplate>
                    <DataTemplate>
                        <Frame Margin="10,5" Padding="15" CornerRadius="10">
                            <VerticalStackLayout>
                                <Label Text="{Binding Content}" FontSize="14"/>
                                <Label Text="{Binding CreatedAt, StringFormat='{}{0:MMM dd, HH:mm}'}" 
                                       TextColor="Gray" FontSize="11" Margin="0,5,0,0"/>
                            </VerticalStackLayout>
                        </Frame>
                    </DataTemplate>
                </CollectionView.ItemTemplate>
            </CollectionView>
            
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

### 7.2 ProfilePage.xaml.cs
```csharp
public partial class ProfilePage : ContentPage
{
    private readonly ApiService _api;

    public ProfilePage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        var account = await _api.GetCurrentUserAsync();
        
        AvatarLabel.Text = account.DisplayName.Length > 0 ? account.DisplayName[0].ToString() : "?";
        DisplayNameLabel.Text = account.DisplayName;
        UsernameLabel.Text = $"@{account.Username}";
        PostsCount.Text = account.PostCount.ToString();
        FollowersCount.Text = account.FollowerCount.ToString();
        FollowingCount.Text = account.FollowingCount.ToString();
        BioLabel.Text = account.Bio;
        
        var posts = await _api.GetAccountPostsAsync(account.Id);
        UserPostsList.ItemsSource = posts;
    }

    private void OnLogoutClicked(object sender, EventArgs e)
    {
        _api.Logout();
        App.Current.MainPage = new NavigationPage(new LoginPage());
    }
}
```

---

## 8. Create Notifications Page

### 8.1 NotificationsPage.xaml
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             x:Class="SocialMediaSimulator.Client.Pages.NotificationsPage"
             Title="Notifications"
             Appearing="OnAppearing">
    
    <RefreshView x:Name="RefreshView" Refreshing="OnRefresh">
        <CollectionView x:Name="NotificationsList">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Frame Margin="10" Padding="15" CornerRadius="10"
                           BackgroundColor="{Binding IsRead, Converter={StaticResource BoolToColorConverter}}">
                        <VerticalStackLayout>
                            <Label Text="{Binding Title}" FontAttributes="Bold"/>
                            <Label Text="{Binding Message}" FontSize="13"/>
                            <Label Text="{Binding CreatedAt, StringFormat='{}{0:MMM dd, HH:mm}'}" 
                                   TextColor="Gray" FontSize="11" Margin="0,5,0,0"/>
                        </VerticalStackLayout>
                    </Frame>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
    </RefreshView>
</ContentPage>
```

### 8.2 NotificationsPage.xaml.cs
```csharp
public partial class NotificationsPage : ContentPage
{
    private readonly ApiService _api;

    public NotificationsPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadNotificationsAsync();
    }

    private async Task LoadNotificationsAsync()
    {
        var notifications = await _api.GetNotificationsAsync(50);
        NotificationsList.ItemsSource = notifications;
    }

    private async void OnRefresh(object sender, EventArgs e)
    {
        await LoadNotificationsAsync();
        RefreshView.IsRefreshing = false;
    }
}
```

---

## 9. Create Communities Page

### 9.1 CommunitiesPage.xaml
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             x:Class="SocialMediaSimulator.Client.Pages.CommunitiesPage"
             Title="Communities"
             Appearing="OnAppearing">
    
    <RefreshView x:Name="RefreshView" Refreshing="OnRefresh">
        <CollectionView x:Name="CommunitiesList">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Frame Margin="10" Padding="15" CornerRadius="10">
                        <VerticalStackLayout>
                            <HorizontalStackLayout>
                                <Frame WidthRequest="50" HeightRequest="50" CornerRadius="25" 
                                       BackgroundColor="LightBlue" Padding="0">
                                    <Label Text="{Binding Name[0]}" FontSize="24" 
                                           HorizontalOptions="Center" VerticalOptions="Center"/>
                                </Frame>
                                <VerticalStackLayout Margin="15,0,0,0">
                                    <Label Text="{Binding Name}" FontSize="18" FontAttributes="Bold"/>
                                    <Label Text="{Binding MemberCount, StringFormat='{0} members'}" 
                                           TextColor="Gray"/>
                                </VerticalStackLayout>
                            </HorizontalStackLayout>
                            
                            <Label Text="{Binding Description}" Margin="0,10,0,0"/>
                            
                            <Button Text="{Binding IsJoined, Converter={StaticResource JoinLeaveConverter}}" 
                                    Clicked="OnJoinLeaveClicked" CommandParameter="{Binding}"
                                    Margin="0,10,0,0"/>
                        </VerticalStackLayout>
                    </Frame>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
    </RefreshView>
</ContentPage>
```

### 9.2 CommunitiesPage.xaml.cs
```csharp
public partial class CommunitiesPage : ContentPage
{
    private readonly ApiService _api;

    public CommunitiesPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCommunitiesAsync();
    }

    private async Task LoadCommunitiesAsync()
    {
        var communities = await _api.GetCommunitiesAsync();
        CommunitiesList.ItemsSource = communities;
    }

    private async void OnRefresh(object sender, EventArgs e)
    {
        await LoadCommunitiesAsync();
        RefreshView.IsRefreshing = false;
    }

    private async void OnJoinLeaveClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Community community)
        {
            if (community.IsJoined)
                await _api.LeaveCommunityAsync(community.Id);
            else
                await _api.JoinCommunityAsync(community.Id);
            
            await LoadCommunitiesAsync();
        }
    }
}
```

---

## 10. Create Search Page

### 10.1 SearchPage.xaml
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             x:Class="SocialMediaSimulator.Client.Pages.SearchPage"
             Title="Search">
    
    <VerticalStackLayout>
        <!-- Search Bar -->
        <SearchBar x:Name="SearchBar" Placeholder="Search accounts..." 
                   SearchButtonPressed="OnSearchPressed" Margin="10"/>
        
        <!-- Trends Section -->
        <Label Text="Trending" FontSize="18" FontAttributes="Bold" Margin="15,10,0,5"/>
        <CollectionView x:Name="TrendsList" HeightRequest="150">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Frame Margin="10,5" Padding="10" CornerRadius="8" BackgroundColor="LightGray">
                        <Label Text="{Binding Name}" FontAttributes="Bold"/>
                    </Frame>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
        
        <!-- Search Results -->
        <Label Text="Results" FontSize="18" FontAttributes="Bold" Margin="15,20,0,5"/>
        <CollectionView x:Name="SearchResultsList">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Frame Margin="10,5" Padding="15" CornerRadius="10">
                        <HorizontalStackLayout>
                            <Frame WidthRequest="40" HeightRequest="40" CornerRadius="20" 
                                   BackgroundColor="LightBlue" Padding="0">
                                <Label Text="{Binding DisplayName[0]}" 
                                       HorizontalOptions="Center" VerticalOptions="Center"/>
                            </Frame>
                            <VerticalStackLayout Margin="10,0,0,0">
                                <Label Text="{Binding DisplayName}" FontAttributes="Bold"/>
                                <Label Text="{Binding Username, StringFormat='@{0}'}" TextColor="Gray"/>
                            </VerticalStackLayout>
                        </HorizontalStackLayout>
                    </Frame>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
        
    </VerticalStackLayout>
</ContentPage>
```

### 10.2 SearchPage.xaml.cs
```csharp
public partial class SearchPage : ContentPage
{
    private readonly ApiService _api;

    public SearchPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTrendsAsync();
    }

    private async Task LoadTrendsAsync()
    {
        var trends = await _api.GetTrendsAsync(10);
        TrendsList.ItemsSource = trends;
    }

    private async void OnSearchPressed(object sender, EventArgs e)
    {
        var query = SearchBar.Text?.Trim();
        if (string.IsNullOrEmpty(query)) return;

        // TODO: Implement search API call
        // var results = await _api.SearchAsync(query);
        // SearchResultsList.ItemsSource = results;
    }
}
```

---

## 11. Create Comments Page

### 11.1 CommentsPage.xaml
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             x:Class="SocialMediaSimulator.Client.Pages.CommentsPage"
             Title="Comments">
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- Post -->
        <Frame Margin="10" Padding="15" CornerRadius="10" Grid.Row="0">
            <VerticalStackLayout>
                <Label Text="{Binding AuthorDisplayName}" FontAttributes="Bold"/>
                <Label Text="{Binding Content}" Margin="0,10,0,0"/>
            </VerticalStackLayout>
        </Frame>
        
        <!-- Comments List -->
        <CollectionView x:Name="CommentsList" Grid.Row="0" Margin="0,100,0,60">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Frame Margin="10,5" Padding="10" CornerRadius="8">
                        <VerticalStackLayout>
                            <Label Text="{Binding AuthorDisplayName}" FontAttributes="Bold" FontSize="12"/>
                            <Label Text="{Binding Content}" FontSize="13"/>
                            <Label Text="{Binding CreatedAt, StringFormat='{}{0:HH:mm}'}" 
                                   TextColor="Gray" FontSize="10"/>
                        </VerticalStackLayout>
                    </Frame>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
        
        <!-- Add Comment -->
        <HorizontalStackLayout Grid.Row="1" Padding="10" BackgroundColor="LightGray">
            <Entry x:Name="CommentEntry" Placeholder="Add a comment..." 
                   HorizontalOptions="FillAndExpand"/>
            <Button Text="Send" Clicked="OnSendCommentClicked" Margin="10,0,0,0"/>
        </HorizontalStackLayout>
        
    </Grid>
</ContentPage>
```

### 11.2 CommentsPage.xaml.cs
```csharp
public partial class CommentsPage : ContentPage
{
    private readonly Post _post;
    private readonly ApiService _api;

    public CommentsPage(Post post, ApiService api)
    {
        InitializeComponent();
        _post = post;
        _api = api;
        BindingContext = post;
    }

    private async void OnSendCommentClicked(object sender, EventArgs e)
    {
        var content = CommentEntry.Text?.Trim();
        if (string.IsNullOrEmpty(content)) return;

        var comment = await _api.AddCommentAsync(_post.Id, content);
        CommentEntry.Text = "";
        // TODO: Reload comments
    }
}
```

---

## 12. Update App.xaml with Styles

### App.xaml
```xml
<?xml version="1.0" encoding="utf-8" ?>
<Application xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="SocialMediaSimulator.Client.App">
    
    <Application.Resources>
        <ResourceDictionary>
            <Style x:Key="SecondaryButton" TargetType="Button">
                <Setter Property="BackgroundColor" Value="LightGray"/>
                <Setter Property="TextColor" Value="Black"/>
            </Style>
            
            <Style x:Key="GhostButton" TargetType="Button">
                <Setter Property="BackgroundColor" Value="Transparent"/>
                <Setter Property="TextColor" Value="Gray"/>
                <Setter Property="Padding" Value="5"/>
            </Style>
            
            <converters:BoolToColorConverter x:Key="BoolToColorConverter" 
                                             xmlns:converters="clr-namespace:SocialMediaSimulator.Client.Converters"/>
            <converters:JoinLeaveConverter x:Key="JoinLeaveConverter"
                                           xmlns:converters="clr-namespace:SocialMediaSimulator.Client.Converters"/>
        </ResourceDictionary>
    </Application.Resources>
    
</Application>
```

---

## 13. Update MauiProgram.cs

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Register services
        builder.Services.AddSingleton<AppConfig>();
        builder.Services.AddHttpClient<ApiService>();

        return builder.Build();
    }
}
```

---

## 14. Update App.xaml.cs for Auth Flow

```csharp
public partial class App : Application
{
    protected override void OnStart()
    {
        base.OnStart();
        
        // Check if user is logged in
        var api = new ApiService(new HttpClient(), new AppConfig());
        var savedToken = GetSavedAuthToken();
        
        if (string.IsNullOrEmpty(savedToken))
        {
            // Not logged in, show login
            MainPage = new NavigationPage(new LoginPage());
        }
        else
        {
            // Logged in, show main app
            api.SetAuthToken(savedToken);
            MainPage = new AppShell();
        }
    }
    
    private string GetSavedAuthToken()
    {
        // TODO: Implement secure storage
        return Preferences.Get("AuthToken", null);
    }
    
    private void SaveAuthToken(string token)
    {
        Preferences.Set("AuthToken", token);
    }
}
```

---

## 15. Ensure Android Manifest Allows HTTP

### AndroidManifest.xml
```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
	<application 
		android:allowBackup="true" 
		android:icon="@mipmap/appicon" 
		android:roundIcon="@mipmap/appicon_round" 
		android:supportsRtl="true"
		android:usesCleartextTraffic="true">
	</application>
	<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
	<uses-permission android:name="android.permission.INTERNET" />
</manifest>
```

---

## 16. Build and Test

### Build Android APK
```bash
cd Client
dotnet build -f net10.0-android -c Release
```

### Install on Device
```bash
adb install -r bin/Release/net10.0-android/com.companyname.socialmediasimulator.apk
```

---

## 17. Git & README

### README.md Update

Add "Android UI (Part 24)" section:

```markdown
## Android App Features (Part 24)

### Screens Implemented
- **Login/Register** - Account creation and authentication
- **Feed** - Main timeline with posts from followed accounts
- **Create Post** - Create new posts with content
- **Profile** - View and edit your profile
- **Notifications** - View engagement notifications
- **Communities** - Browse and join communities
- **Search** - Find accounts and view trends
- **Comments** - View and add comments to posts

### Architecture
- MVVM pattern
- ApiService for all API calls
- Secure token storage
- Pull-to-refresh on all lists

### Building
```bash
cd Client
dotnet build -f net10.0-android -c Release
```
```

### Git Commit

```bash
git add .
git commit -m "Part 24: Android UI Implementation - Make app usable"
git push
```

---

## 18. DELIVERABLES

After this part:

1. ✅ User can register and login
2. ✅ User sees feed with posts
3. ✅ User can create posts
4. ✅ User can like posts
5. ✅ User can comment on posts
6. ✅ User can view profile
7. ✅ User can see notifications
8. ✅ User can browse communities
9. ✅ User can join/leave communities
10. ✅ User can view trends
11. ✅ User can logout

---

## 19. FINAL SESSION REPORT

```text
# PART 24 — COMPLETE

## 1. Android Screens Implemented
- [ ] Login/Register
- [ ] Feed with posts
- [ ] Create post
- [ ] Profile page
- [ ] Notifications
- [ ] Communities
- [ ] Search
- [ ] Comments

## 2. API Integration
- [ ] Auth endpoints
- [ ] Feed endpoints
- [ ] Post endpoints
- [ ] Account endpoints
- [ ] Notification endpoints
- [ ] Community endpoints
- [ ] News endpoints
- [ ] Trends endpoints

## 3. App Features
- [ ] Bottom tab navigation
- [ ] Pull-to-refresh
- [ ] Authentication flow
- [ ] Token persistence

## 4. Git
Commit: ...
Push: ...
Verified: YES
Working tree: clean

## 5. Current Project Status
01A-24 COMPLETE

## 6. NEXT
NEXT: PART 25 — Backend Enhancement / Polish
```

**STOP after completing Part 24 and reporting the session log.**
