using System;
using System.Collections.Generic;
using System.Text;

namespace WoTMapWPF.Services
{
    public class NotificationService
    {
        public event Action<NotificationMessage>? Notify;

        public void DoNotify(NotificationMessage notificationMessage)
        {
            Notify?.Invoke(notificationMessage);
        }
    }

    public struct NotificationMessage
    {
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public int HideDelay { get; set; }
    }

    public enum NotificationType
    {
        Show,
        ShowError,
        ShowAndHide,
        ShowErrorAndHide
    }
}
