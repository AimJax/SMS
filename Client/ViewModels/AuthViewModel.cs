using System.Windows.Input;
using SocialMediaSimulator.Client.Services;
using SocialMediaSimulator.Client.Models;

namespace SocialMediaSimulator.Client.ViewModels;

public class AuthViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _username = string.Empty;
    private string _displayName = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading;
    private bool _isRegistering;

    public event EventHandler<(Account Account, string Token)>? AuthSucceeded;

    public AuthViewModel(ApiService apiService)
    {
        _apiService = apiService;
        LoginCommand = new Command(async () => await LoginAsync());
        RegisterCommand = new Command(async () => await RegisterAsync());
        ToggleModeCommand = new Command(() => IsRegistering = !IsRegistering);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsRegistering
    {
        get => _isRegistering;
        set => SetProperty(ref _isRegistering, value);
    }

    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }
    public ICommand ToggleModeCommand { get; }

    async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter email and password";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var response = await _apiService.LoginAsync(Email, Password);
            if (response?.Success == true)
            {
                var account = await _apiService.GetCurrentAccountAsync();
                var token = response!.Token!;
                AuthSucceeded?.Invoke(this, (account!, token));
            }
            else
            {
                ErrorMessage = response?.Error ?? "Login failed";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) || 
            string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(DisplayName))
        {
            ErrorMessage = "Please fill in all fields";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var response = await _apiService.RegisterAsync(Username, DisplayName, Email, Password);
            if (response?.Success == true)
            {
                var account = await _apiService.GetCurrentAccountAsync();
                var token = response!.Token!;
                AuthSucceeded?.Invoke(this, (account!, token));
            }
            else
            {
                ErrorMessage = response?.Error ?? "Registration failed";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

/// <summary>
/// Simple ICommand implementation
/// </summary>
public class Command : ICommand
{
    private readonly Func<Task> _execute;
    private bool _isExecuting;

    public Command(Func<Task> execute) => _execute = execute;
    public Command(Action execute) => _execute = () => { execute(); return Task.CompletedTask; };

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => !_isExecuting;

    public async void Execute(object? parameter)
    {
        if (_isExecuting) return;
        _isExecuting = true;
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        try { await _execute(); }
        finally { _isExecuting = false; CanExecuteChanged?.Invoke(this, EventArgs.Empty); }
    }
}
