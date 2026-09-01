using SocialMediaSimulator.Client.ViewModels;
using SocialMediaSimulator.Client.Models;

namespace SocialMediaSimulator.Client.Views;

public partial class AuthShell : ContentPage
{
    private readonly Entry _usernameEntry;
    private readonly Entry _displayNameEntry;
    private readonly Entry _bioEntry;
    private readonly Label _errorLabel;
    private readonly ActivityIndicator _loadingIndicator;
    private readonly Button _submitButton;

    public AuthShell()
    {
        Title = "Create Account";
        BackgroundColor = Color.FromArgb("#1A1A24");
        
        var mainLayout = new VerticalStackLayout { Padding = 20, Spacing = 15 };

        // Header
        mainLayout.Add(new Label 
        { 
            Text = "Create Your Account", 
            FontSize = 28, 
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#F5F5F7"),
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 50, 0, 10)
        });
        mainLayout.Add(new Label 
        { 
            Text = "Set up your social media profile", 
            FontSize = 14,
            TextColor = Color.FromArgb("#9E9EA8"),
            HorizontalOptions = LayoutOptions.Center
        });

        // Form
        var formLayout = new VerticalStackLayout { Spacing = 15 };

        _usernameEntry = new Entry 
        { 
            Placeholder = "Username", 
            TextColor = Color.FromArgb("#F5F5F7"), 
            PlaceholderColor = Color.FromArgb("#606070"), 
            BackgroundColor = Color.FromArgb("#242430"),
            HeightRequest = 50
        };
        
        _displayNameEntry = new Entry 
        { 
            Placeholder = "Display Name", 
            TextColor = Color.FromArgb("#F5F5F7"), 
            PlaceholderColor = Color.FromArgb("#606070"), 
            BackgroundColor = Color.FromArgb("#242430"),
            HeightRequest = 50
        };
        
        _bioEntry = new Entry 
        { 
            Placeholder = "Bio (optional)", 
            TextColor = Color.FromArgb("#F5F5F7"), 
            PlaceholderColor = Color.FromArgb("#606070"), 
            BackgroundColor = Color.FromArgb("#242430"),
            HeightRequest = 50
        };

        _errorLabel = new Label { TextColor = Color.FromArgb("#E53935"), FontSize = 12, IsVisible = false };
        _loadingIndicator = new ActivityIndicator { IsRunning = false, HorizontalOptions = LayoutOptions.Center };

        _submitButton = new Button 
        { 
            Text = "Enter Game",
            BackgroundColor = Color.FromArgb("#F5A623"),
            TextColor = Color.FromArgb("#1A1A10"),
            HeightRequest = 50
        };
        _submitButton.Clicked += (_, _) => CreateAccountAndEnterAsync();

        formLayout.Add(_usernameEntry);
        formLayout.Add(_displayNameEntry);
        formLayout.Add(_bioEntry);
        formLayout.Add(_errorLabel);
        formLayout.Add(_submitButton);
        formLayout.Add(_loadingIndicator);

        mainLayout.Add(formLayout);

        Content = new ScrollView { Content = mainLayout };
    }

    async void CreateAccountAndEnterAsync()
    {
        var username = _usernameEntry.Text?.Trim() ?? "";
        var displayName = _displayNameEntry.Text?.Trim() ?? "";
        var bio = _bioEntry.Text?.Trim() ?? "";

        if (string.IsNullOrEmpty(username))
        {
            _errorLabel.Text = "Username is required";
            _errorLabel.IsVisible = true;
            return;
        }

        if (string.IsNullOrEmpty(displayName))
        {
            _errorLabel.Text = "Display Name is required";
            _errorLabel.IsVisible = true;
            return;
        }

        _errorLabel.IsVisible = false;
        _loadingIndicator.IsRunning = true;
        _submitButton.IsEnabled = false;

        try
        {
            // Create fake account locally and navigate to MainShell
            var fakeAccount = new Account
            {
                AccountId = Guid.NewGuid(),
                Username = username,
                Profile = new AccountProfile
                {
                    DisplayName = displayName,
                    Bio = bio,
                    FollowerCount = 0,
                    FollowingCount = 0
                }
            };

            // Set authenticated and navigate
            App.SetAuthenticated(fakeAccount, "fake-token");
            
            if (Application.Current.Windows.Count > 0)
            {
                Application.Current.Windows[0].Page = new MainShell();
            }
        }
        catch (Exception ex)
        {
            _errorLabel.Text = $"Error: {ex.Message}";
            _errorLabel.IsVisible = true;
        }
        finally
        {
            _loadingIndicator.IsRunning = false;
            _submitButton.IsEnabled = true;
        }
    }
}
