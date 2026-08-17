using System;
using System.Collections.Generic;
using System.Text;

namespace WoTMapWPF.Services
{
    public record class NotificationMessage(string Message, NotificationType Type, int HideDelay = 3000);

    public enum NotificationType
    {
        Show,
        ShowError,
        ShowAndHide,
        ShowErrorAndHide
    }
}
