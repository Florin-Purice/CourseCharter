using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Threading;
using System.Threading.Tasks;
using WoTMapWPF.Services;

namespace WoTMapWPF.CustomControls
{
    public partial class NotificationControlViewModel : ViewModelBase, IRecipient<NotificationMessage>
    {
        private CancellationTokenSource? cancellationTokenSource;

        public NotificationControlViewModel(IMessenger messenger)
        {
            messenger.Register<NotificationMessage>(this);
        }

        [ObservableProperty]
        public partial string Text { get; set; } = string.Empty;
        [ObservableProperty]
        public partial bool IsVisible { get; set; } = false;
        [ObservableProperty]
        public partial bool IsError { get; set; } = false;

        [RelayCommand]
        public void Close()
        {
            IsVisible = false;
        }

        public void Receive(NotificationMessage message)
        {
            switch (message.Type)
            {
                case NotificationType.Show:
                    ShowNotification(message.Message);
                    break;
                case NotificationType.ShowError:
                    ShowError(message.Message);
                    break;
                case NotificationType.ShowAndHide:
                    ShowNotificationAndHide(message.Message);
                    break;
                case NotificationType.ShowErrorAndHide:
                    ShowErrorAndHide(message.Message);
                    break;
            }
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
