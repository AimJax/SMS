using SocialMediaSimulator.Client.ViewModels;
using SocialMediaSimulator.Client.Models;

namespace SocialMediaSimulator.Client.Views;

public partial class AuthShell : ContentPage
{
    private readonly AuthViewModel _viewModel;
    private readonly Entry _emailEntry;
    private readonly Entry _passwordEntry;
    private readonly Entry _usernameEntry;
    private readonly Entry _displayNameEntry;
    private readonly Label _errorLabel;
    private readonly ActivityIndicator _loadingIndicator;
    private readonly Button _submitButton;
    private readonly Button _toggleButton;
    private readonly VerticalStackLayout _registerFields;

    public AuthShell()
    {
        _viewModel = new AuthViewModel(App.ApiService);
        _viewModel.AuthSucceeded += OnAuthSucceeded;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        var mainLayout = new VerticalStackLayout { Padding = 20, Spacing = 15 };

        // Header
        mainLayout.Add(new Label 
        { 
            Text = "Social Media Simulator", 
            FontSize = 28, 
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 50, 0, 10)
        });
        mainLayout.Add(new Label 
        { 
            Text = "Connect with NPCs, trends, and more", 
            FontSize = 14,
            TextColor = Colors.Gray,
            HorizontalOptions = LayoutOptions.Center
        });

        // Form
        var formFrame = new Frame 
        { 
            Padding = 20,
            Margin = new Thickness(0, 30, 0, 0)
        };
        var formLayout = new VerticalStackLayout { Spacing = 15 };

        _emailEntry = new Entry { Placeholder = "Email", Keyboard = Keyboard.Email };
        _passwordEntry = new Entry { Placeholder = "Password", IsPassword = true };
        _usernameEntry = new Entry { Placeholder = "Username" };
        _usernameEntry.IsVisible = false;
        _displayNameEntry = new Entry { Placeholder = "Display Name" };
        _displayNameEntry.IsVisible = false;
        _errorLabel = new Label { TextColor = Colors.Red, FontSize = 12, IsVisible = false };
        _loadingIndicator = new ActivityIndicator { IsRunning = false, HorizontalOptions = LayoutOptions.Center };

        _submitButton = new Button { Text = "Sign In" };
        _submitButton.Clicked += (_, _) => SubmitAsync();

        _toggleButton = new Button 
        { 
            Text = "Need an account? Sign Up",
            BackgroundColor = Colors.Transparent,
            TextColor = Colors.Blue
        };
        _toggleButton.Clicked += (_, _) => ToggleMode();

        formLayout.Add(_emailEntry);
        formLayout.Add(_passwordEntry);
        formLayout.Add(_usernameEntry);
        formLayout.Add(_displayNameEntry);
        formLayout.Add(_errorLabel);
        formLayout.Add(_submitButton);
        formLayout.Add(_toggleButton);
        formLayout.Add(_loadingIndicator);

        formFrame.Content = formLayout;
        mainLayout.Add(formFrame);

        Content = new ScrollView { Content = mainLayout };
    }

    void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_viewModel.IsRegistering):
                _usernameEntry.IsVisible = _viewModel.IsRegistering;
                _displayNameEntry.IsVisible = _viewModel.IsRegistering;
                _submitButton.Text = _viewModel.IsRegistering ? "Sign Up" : "Sign In";
                _toggleButton.Text = _viewModel.IsRegistering ? "Have an account? Sign In" : "Need an account? Sign Up";
                break;
            case nameof(_viewModel.IsLoading):
                _loadingIndicator.IsRunning = _viewModel.IsLoading;
                _submitButton.IsEnabled = !_viewModel.IsLoading;
                break;
            case nameof(_viewModel.ErrorMessage):
                _errorLabel.Text = _viewModel.ErrorMessage;
                _errorLabel.IsVisible = !string.IsNullOrEmpty(_viewModel.ErrorMessage);
                break;
        }
    }

    void SubmitAsync()
    {
        _viewModel.Email = _emailEntry.Text ?? "";
        _viewModel.Password = _passwordEntry.Text ?? "";
        _viewModel.Username = _usernameEntry.Text ?? "";
        _viewModel.DisplayName = _displayNameEntry.Text ?? "";

        if (_viewModel.IsRegistering)
            _viewModel.RegisterCommand.Execute(null);
        else
            _viewModel.LoginCommand.Execute(null);
    }

    void ToggleMode()
    {
        _viewModel.IsRegistering = !_viewModel.IsRegistering;
    }

    void OnAuthSucceeded(object? sender, (Account Account, string Token) data)
    {
        App.SetAuthenticated(data.Account, data.Token);
        if (Application.Current.Windows.Count > 0)
        {
            Application.Current.Windows[0].Page = new MainShell();
        }
    }
}
