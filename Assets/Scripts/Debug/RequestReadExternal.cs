using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class RequestReadExternal : MonoBehaviour
{
    private void Start() => RequestStoragePermissions();

    private void RequestStoragePermissions()
    {
#if UNITY_ANDROID
        if (Application.platform != RuntimePlatform.Android)
        {
            Debug.Log("ℹ️ Not running on Android—no storage permissions needed.");
            return;
        }

        // Get the current SDK_INT via android.os.Build$VERSION
        int sdkInt;
        using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            sdkInt = version.GetStatic<int>("SDK_INT");
        }

        // If API>=30, check MANAGE_EXTERNAL_STORAGE status
        bool isManager = false;
        if (sdkInt >= 30)
        {
            using (var env = new AndroidJavaClass("android.os.Environment"))
            {
                isManager = env.CallStatic<bool>("isExternalStorageManager");
            }
        }

        // Set up unified callbacks
        var callbacks = new PermissionCallbacks();
        callbacks.PermissionGranted += perm => Debug.Log($"✅ Permission granted: {perm}");
        callbacks.PermissionDenied += perm => Debug.LogWarning($"⚠️ Permission denied: {perm}");
        callbacks.PermissionDeniedAndDontAskAgain += perm => Debug.LogError($"🚫 Permission denied permanently: {perm}");

        // Request the right permission group
        if (sdkInt >= 33)
        {
            // Android 13+: granular media permissions
            Debug.Log("ℹ️ Requesting Android 13+ media permissions");
            Permission.RequestUserPermissions(new[]
            {
                "android.permission.READ_MEDIA_IMAGES",
                "android.permission.READ_MEDIA_VIDEO",
                "android.permission.READ_MEDIA_AUDIO"
            }, callbacks);
        }
        else if (sdkInt >= 30)
        {
            // Android 11–12: all-files access
            const string manageAll = "android.permission.MANAGE_EXTERNAL_STORAGE";
            if (!isManager)
            {
                Debug.Log("ℹ️ Requesting MANAGE_EXTERNAL_STORAGE (All files access)");
                Permission.RequestUserPermission(manageAll, callbacks);
            }
            else
            {
                Debug.Log("✅ MANAGE_EXTERNAL_STORAGE already granted");
            }
        }
        else
        {
            // Android 6–10: legacy READ/WRITE
            Debug.Log("ℹ️ Requesting legacy READ & WRITE external-storage");
            Permission.RequestUserPermissions(new[]
            {
                Permission.ExternalStorageRead,
                Permission.ExternalStorageWrite
            }, callbacks);
        }
#else
        Debug.Log("ℹ️ External-storage permissions not required on this platform.");
#endif
    }
}
