using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Notifications.Android;
using UnityEngine;
using UnityEngine.Android;

public class AndroidNotifications : MonoBehaviour
{
    public void RequestAuthorization()
    {
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
    }

    public void RegisterNotifcationChannel()
    {
        var channel = new AndroidNotificationChannel
        {
            Id = "default_channel",
            Name = "Default Channel",
            Importance = Importance.Default,
            Description = "New Exhibition Live"
        };
        
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }

    public void ScheduleNotification(string title, string body, DateTime fireTime)
    {
        var notification = new AndroidNotification
        {
            Title = title,
            Text = body,
            FireTime = fireTime
        };
        //notification.SmallIcon = "default_icon";
        //notification.LargeIcon = "default_icon";
        AndroidNotificationCenter.SendNotification(notification, "default_channel");
    }

    public void PlayNotification(string title, string body)
    {
        var notification = new AndroidNotification
        {
            Title = title,
            Text = body,
            FireTime = DateTime.Now
        };
        
        AndroidNotificationCenter.SendNotification(notification, "default_channel");
    }
}
