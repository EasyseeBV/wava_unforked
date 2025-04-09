using UnityEngine;
#if UNITY_IOS
using System.Collections;
using Unity.Notifications.iOS;
#endif
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class NotificationsRequest : MonoBehaviour
{
    private void Awake()
    {
#if UNITY_IOS
        StartCoroutine(RequestIOSNotificationPermission());
#endif

#if UNITY_ANDROID
        RequestAndroidNotificationPermission();
#endif
    }
    
#if UNITY_IOS
    private IEnumerator RequestIOSNotificationPermission()
    {
        // Use the new enum and request class.
        var options = AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound;
        using (var req = new AuthorizationRequest(options, true))
        {
            while (!req.IsFinished)
            {
                yield return null;
            }

            if (req.Granted)
            {
                Debug.Log("iOS notification permission granted");
            }
            else
            {
                Debug.Log("iOS notification permission denied");
            }
        }
    }
#endif

#if UNITY_ANDROID
    private void RequestAndroidNotificationPermission()
    {
        // On Android 13 (API level 33) and above, the POST_NOTIFICATIONS permission is required.
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
            callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
            callbacks.PermissionDeniedAndDontAskAgain += PermissionCallbacks_PermissionDeniedAndDontAskAgain;
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS", callbacks);
        }
    }

    private void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionDeniedAndDontAskAgain");
    }

    private void PermissionCallbacks_PermissionGranted(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionGranted");
    }

    private void PermissionCallbacks_PermissionDenied(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionDenied");
    }
#endif
}
