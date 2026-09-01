using System.Collections.ObjectModel;
using System.Windows.Input;
using SocialMediaSimulator.Client.Services;
using SocialMediaSimulator.Client.Models;

namespace SocialMediaSimulator.Client.ViewModels;

public class NotificationsViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private ObservableCollection<Notification> _notifications = new();
    private bool _isLoading;
    private int _unreadCount;

    public event EventHandler<int>? AccountSelected;
    public event EventHandler<Guid>? PostSelected;

    public NotificationsViewModel(ApiService apiService)
    {
        _apiService = apiService;
        RefreshCommand = new Command(async () => await LoadAsync());
        MarkReadCommand = new Command<Notification>(async (n) => await MarkReadAsync(n));
        MarkAllReadCommand = new Command(async () => await MarkAllReadAsync());
        NotificationTappedCommand = new Command<Notification>(OnNotificationTapped);
    }

    public ObservableCollection<Notification> Notifications
    {
        get => _notifications;
        set => SetProperty(ref _notifications, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public int UnreadCount
    {
        get => _unreadCount;
        set => SetProperty(ref _unreadCount, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand MarkReadCommand { get; }
    public ICommand MarkAllReadCommand { get; }
    public ICommand NotificationTappedCommand { get; }

    public async Task LoadAsync()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            var notifications = await _apiService.GetNotificationsAsync();
            Notifications = new ObservableCollection<Notification>(notifications ?? new List<Notification>());
            UnreadCount = Notifications.Count(n => !n.IsRead);
        }
        finally
        {
            IsLoading = false;
        }
    }

    async Task MarkReadAsync(Notification notification)
    {
        if (!notification.IsRead)
        {
            await _apiService.MarkNotificationReadAsync(notification.NotificationId);
            notification.IsRead = true;
            UnreadCount = Math.Max(0, UnreadCount - 1);
        }
    }

    async Task MarkAllReadAsync()
    {
        await _apiService.MarkAllNotificationsReadAsync();
        foreach (var n in Notifications)
            n.IsRead = true;
        UnreadCount = 0;
        OnPropertyChanged(nameof(Notifications));
    }

    void OnNotificationTapped(Notification notification)
    {
        if (!notification.IsRead)
        {
            _ = MarkReadAsync(notification);
        }

        if (notification.RelatedAccountId.HasValue)
        {
            AccountSelected?.Invoke(this, notification.RelatedAccountId.Value);
        }
        else if (notification.RelatedPostId.HasValue)
        {
            PostSelected?.Invoke(this, notification.RelatedPostId.Value);
        }
    }
}
