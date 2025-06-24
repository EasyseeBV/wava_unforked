using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ARLoader : MonoBehaviour
{
    private static ARLoader instance;

    [SerializeField] private GameObject accessModal;
    [SerializeField] private Button openSettingsButton;

    private void Awake()
    {
        if (instance == null) instance = this;
        
        openSettingsButton.onClick.AddListener(OpenSettings);
        accessModal.SetActive(false);
    }

    public static void Open(ArtworkData artwork, float _distance = 12)
    {
        instance.StartCoroutine(CheckAndRequestCameraPermission(result =>
        {
            if (result)
            {
                ArTapper.ArtworkToPlace = artwork;
                ArTapper.PlaceDirectly = false; // old system?
                ArTapper.DistanceWhenActivated = _distance;

                if(string.IsNullOrEmpty(artwork.alt_scene))
                {
                    Debug.Log("Loading Default AR Scene");
                    SceneManager.LoadScene("ARView");
                }
                else
                {
                    Debug.Log("Loading Alternate Scene " + artwork.alt_scene);
                    SceneManager.LoadScene(artwork.alt_scene);
                }
            }
            else
            {
                Debug.Log("User does not have camera permissions");
            }
        }));
    }
    
    const string DeclineCountKey = "CameraPermissionDeclineCount";


    private static IEnumerator CheckAndRequestCameraPermission(Action<bool> onResult)
    {
        #if UNITY_IOS
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            onResult(true);
            yield break;
        }
        #elif UNITY_ANDROID
        if (Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            onResult(true);
            yield break;
        }
        #endif
        
        bool completed = false;
        bool granted  = false;

        #if UNITY_IOS
        var req = Application.RequestUserAuthorization(UserAuthorization.WebCam);
        yield return req;
        granted = Application.HasUserAuthorization(UserAuthorization.WebCam);
        completed = true;

        #elif UNITY_ANDROID
        var callbacks = new PermissionCallbacks();
        callbacks.PermissionGranted += name =>
        {
            if (name == Permission.Camera)
            {
                granted   = true;
                completed = true;
            }
        };
        callbacks.PermissionDenied += name =>
        {
            if (name == Permission.Camera)
            {
                granted   = false;
                completed = true;
            }
        };
        callbacks.PermissionDeniedAndDontAskAgain += name =>
        {
            if (name == Permission.Camera)
            {
                granted   = false;
                completed = true;
            }
        };

        Permission.RequestUserPermission(Permission.Camera, callbacks);
        yield return new WaitUntil(() => completed);
        #endif

        if (granted)
        {
            PlayerPrefs.DeleteKey(DeclineCountKey);
            onResult(true);
            yield break;
        }
        
        int declines = PlayerPrefs.GetInt(DeclineCountKey, 0) + 1;
        PlayerPrefs.SetInt(DeclineCountKey, declines);
        PlayerPrefs.Save();
        
        if (declines >= 3)
        {
            OpenSettings();
        }

        onResult(false);
    }

    private static void OpenSettings()
    {
#if UNITY_IOS
            Application.OpenURL("app-settings:");
#elif UNITY_ANDROID
        OpenAndroidAppSettings();
#endif
    }

    #if UNITY_ANDROID
    private static void OpenAndroidAppSettings()
    {
        var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        var settingsClass = new AndroidJavaClass("android.provider.Settings");
        string action = settingsClass.GetStatic<string>("ACTION_APPLICATION_DETAILS_SETTINGS");
        var intent = new AndroidJavaObject("android.content.Intent", action);

        var uriClass = new AndroidJavaClass("android.net.Uri");
        var uri = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", Application.identifier, null);
        intent.Call<AndroidJavaObject>("setData", uri);

        currentActivity.Call("startActivity", intent);
    }
    #endif
}