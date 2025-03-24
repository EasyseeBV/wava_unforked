using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Notifications.iOS;
using UnityEngine;

public class iOSNotifications : MonoBehaviour
{
    public IEnumerator RequestAuthorization()
    {
        using var request = new AuthorizationRequest(AuthorizationOption.Alert | AuthorizationOption.Badge, true);
        while (!request.IsFinished)
        {
            yield return null;
        }
    }

    public void ScheduleNotification(string title, string body, string subtitle, TimeSpan dateTime)
    {
        var timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = dateTime,
            Repeats = false
        };

        var notification = new iOSNotification()
        {
            Identifier = "new_exhibition",
            Title = title,
            Body = body,
            Subtitle = subtitle,
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Badge),
            CategoryIdentifier = "default_category",
            ThreadIdentifier = "thread1",
            Trigger = timeTrigger
        };
        
        iOSNotificationCenter.ScheduleNotification(notification);
    }
}
