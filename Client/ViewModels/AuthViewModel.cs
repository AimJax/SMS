using System.Windows.Input;
using SocialMediaSimulator.Client.Services;
using SocialMediaSimulator.Client.Models;

namespace SocialMediaSimulator.Client.ViewModels;

public class AuthViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private string _username = string.Empty;
    private string _displayName = string.Empty;
    private string _bio = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading;

    public event EventHandler<(Account Account, string Token)>? AuthSucceeded;

    public AuthViewModel(ApiService apiService)
    {
        _apiService = apiService;
        LoginCommand = new Command(async () => await LoginAsync());
        RegisterCommand = new Command(async () => await RegisterAsync());
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

    public string Bio
    {
        get => _bio;
        set => SetProperty(ref _bio, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
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

    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }

    async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter username and password";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var response = await _apiService.LoginAsync(Username, Password);
            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                var account = await _apiService.GetCurrentAccountAsync();
                if (account != null)
                {
                    AuthSucceeded?.Invoke(this, (account, response.Token));
                }
                else
                {
                    ErrorMessage = "Failed to get account info";
                }
            }
            else
            {
                ErrorMessage = "Login failed";
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
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(DisplayName))
        {
            ErrorMessage = "Username and Display Name are required";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var response = await _apiService.RegisterAsync(Username, DisplayName, null, Password ?? "sim123456", Bio);
            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                var account = await _apiService.GetCurrentAccountAsync();
                if (account != null)
                {
                    AuthSucceeded?.Invoke(this, (account, response.Token));
                }
                else
                {
                    // Create account from response
                    var acc = new Account
                    {
                        AccountId = response.Account?.AccountId ?? Guid.NewGuid(),
                        Username = response.Account?.Username ?? Username,
                        Profile = new AccountProfile
                        {
                            DisplayName = response.Account?.DisplayName ?? DisplayName,
                            Bio = response.Account?.Bio ?? Bio
                        }
                    };
                    AuthSucceeded?.Invoke(this, (acc, response.Token));
                }
            }
            else
            {
                ErrorMessage = "Registration failed. Server may not be running.";
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

/// <summary>
/// Generic ICommand with parameter
/// </summary>
public class Command<T> : ICommand
{
    private readonly Action<T?> _execute;

    public Command(Action<T?> execute) => _execute = execute;

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => _execute((T?)parameter);
}
