using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AlmostEngine.Screenshot;
using UnityEngine;
using UnityEngine.Serialization;

public class ScreenshotHandler : MonoBehaviour
{
    [FormerlySerializedAs("m_Camera")]
    public Camera arCamera;
    
    private int m_Width = 1600;
    private int m_Height = 900;
    
    private RenderTexture renderTexture;
    private Texture2D capturedTexture;

    private void Awake()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        using( AndroidJavaClass ajc = new AndroidJavaClass( "com.yasirkula.unity.NativeGalleryMediaPickerFragment" ) )
	        ajc.SetStatic<bool>( "GrantPersistableUriPermission", true );
#endif
    }

    private void Start()
    {
        // Set width and height to screen resolution
        m_Width = Screen.width;
        m_Height = Screen.height;
    }

    public void Capture()
    {
        capturedTexture = SimpleScreenshotCapture.CaptureCameraToTexture(m_Width, m_Height, arCamera);
        SaveCapturedTexture();
    }
    
    private void SaveCapturedTexture()
    {
        if (capturedTexture == null)
        {
            Debug.LogWarning("No texture captured. Please call Capture() first.");
            return;
        }

        byte[] pngData = capturedTexture.EncodeToPNG();
        if (pngData != null)
        {
            string add = DateTime.Now.ToString("yyyyMMddHHmmss");
            string fileName = $"{ArTapper.ArtworkToPlace.title}_{add}.png";
            string path = Path.Combine(AppCache.GalleryFolder, fileName);
            
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                if (directory != null)
                {
                    Directory.CreateDirectory(directory);
                    Debug.Log("Created new directory: " + directory);
                }
            }
            
            File.WriteAllBytes(path, pngData);
            Debug.Log("Temp PNG saved to: " + path);
            
            NativeGallery.SaveImageToGallery(capturedTexture, "WAVA", fileName, (success, result) =>
            {
                Debug.Log($"Moved? [{success}] | path: {result}");

                if (!AppCache.GalleryFilePaths.Contains(result)) return;
                AppCache.GalleryFilePaths.Add(result);
                AppCache.SaveFilePaths();
            });
        }
        else
        {
            Debug.LogError("Failed to encode texture to PNG.");
        }
        
        Destroy(capturedTexture);
    }
}
