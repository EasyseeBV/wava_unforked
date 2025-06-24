using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using AlmostEngine.Screenshot;

public class CaptureCameraToTextureExample : MonoBehaviour
{
    public RawImage m_RawImage;
    public Camera m_Camera;
    public ScreenshotManager m_ScreenshotManager;
    
    private int m_Width = 1600;
    private int m_Height = 900;
    
    private RenderTexture m_RenderTexture;
    private Texture2D m_CapturedTexture;

    private string screenshotPath;
    
    private void Start()
    {
        // Set width and height to screen resolution
        m_Width = Screen.width;
        m_Height = Screen.height;

        screenshotPath = m_ScreenshotManager.GetExportPath();
    }

    public void Capture()
    {
        m_CapturedTexture = SimpleScreenshotCapture.CaptureCameraToTexture(m_Width, m_Height, m_Camera);
        SaveCapturedTexture();
    }
    
    private void SaveCapturedTexture()
    {
        if (m_CapturedTexture == null)
        {
            Debug.LogWarning("No texture captured. Please call Capture() first.");
            return;
        }

        byte[] pngData = m_CapturedTexture.EncodeToPNG();
        if (pngData != null)
        {
            string fileName = "Captured.png";
            string path = Path.Combine(Application.temporaryCachePath, fileName);
            File.WriteAllBytes(path, pngData);
            Debug.Log("Temp PNG saved to: " + path);
        }
        else
        {
            Debug.LogError("Failed to encode texture to PNG.");
        }
        
        Destroy(m_CapturedTexture);
    }
}


