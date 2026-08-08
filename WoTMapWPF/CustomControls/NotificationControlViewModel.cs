using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading;
using System.Threading.Tasks;
using WoTMapWPF.Services;

namespace WoTMapWPF.CustomControls
{
    public partial class NotificationControlViewModel : ViewModelBase
    {
        private CancellationTokenSource? cancellationTokenSource;

        [ObservableProperty]
        private string text = string.Empty;
        [ObservableProperty]
        private bool isVisible = false;
        [ObservableProperty]
        private bool isError = false;

        public NotificationControlViewModel(NotificationService notificationService)
        {
            notificationService.Notify += NotificationService_Notify;
        }

        private void NotificationService_Notify(NotificationMessage obj)
        {
            switch (obj.Type)
            {
                case NotificationType.Show:
                    ShowNotification(obj.Message);
                    break;
                case NotificationType.ShowError:
                    ShowError(obj.Message);
                    break;
                case NotificationType.ShowAndHide:
                    ShowNotificationAndHide(obj.Message);
                    break;
                case NotificationType.ShowErrorAndHide:
                    ShowErrorAndHide(obj.Message);
                    break;
            }
        }

        [RelayCommand]
        public void Close()
        {
            IsVisible = false;
        }

        private void ShowNotification(string message)
        {
            IsError = false;
            Show(message);
        }

        private void ShowError(string message)
        {
            IsError = true;
            Show(message);
        }

        private void ShowNotificationAndHide(string message, int delayMilliseconds = 3000)
        {
            IsError = false;
            ShowAndHide(message, delayMilliseconds);
        }

        private void ShowErrorAndHide(string message, int delayMilliseconds = 3000)
        {
            IsError = true;
            ShowAndHide(message, delayMilliseconds);
        }

        private void Show(string message)
        {
            cancellationTokenSource?.Cancel();
            Text = message;
            IsVisible = true;
        }

        private async void ShowAndHide(string message, int delayMilliseconds)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            Text = message;
            IsVisible = true;
            await Task.Delay(delayMilliseconds);
            if (!cancellationToken.IsCancellationRequested)
                IsVisible = false;
        }
    }
}
