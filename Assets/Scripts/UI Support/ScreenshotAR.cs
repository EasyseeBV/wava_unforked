using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AlmostEngine.Screenshot;
using UnityEngine;

public class ScreenshotAR : MonoBehaviour
{
    [SerializeField] private ScreenshotManager screenshotManager;

    [Header("ScreenCapture")]
    [SerializeField] private GameObject[] objectsToDisable;
    
    public void Capture()
    {
        Handheld.Vibrate();
        StartCoroutine(CaptureScreenshot());
    }

    private IEnumerator CaptureAndSave()
    {
        Camera arCam = Camera.main;
        if (arCam == null)
        {
            Debug.LogError("No Camera.main found for AR capture!");
            yield break;
        }

        // 2) Set up a RenderTexture matching screen dimensions
        int width  = Screen.width;
        int height = Screen.height;
        var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);

        // 3) Redirect camera output to RT and render
        arCam.targetTexture = rt;
        arCam.Render();

        // 4) Read pixels from RT into a Texture2D
        RenderTexture.active = rt;
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        // 5) Cleanup RT
        arCam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // 6) Encode to PNG
        byte[] pngData = tex.EncodeToPNG();

        // 7) Write to disk at your export path
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename  = $"ARShot_{timestamp}.png";
        string path = Path.Combine(screenshotManager.GetExportPath(), "screenshots", filename);//screenshotManager.GetExportPath();
        try
        {
            System.IO.File.WriteAllBytes(path, pngData);
            Debug.Log($"AR screenshot saved to: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to write screenshot: {e.Message}");
        }

        yield return null;
    }

    private IEnumerator CaptureAndSave2()
    {
        yield return new WaitForEndOfFrame();
        var camera = Camera.main;
        var width = Screen.width;
        var height = Screen.height;
        RenderTexture rt = new RenderTexture(width, height, 24);
        
        var currentRT = RenderTexture.active;
        RenderTexture.active = rt;
        
        camera.Render();

        var image = new Texture2D(width, height);
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();
        
        camera.targetTexture = currentRT;
        
        RenderTexture.active = currentRT;
        
        byte[] bytes = image.EncodeToPNG();
        var fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        var filePath = Path.Combine(screenshotManager.GetExportPath(), "screenshots", fileName);

        if (!Directory.Exists(Path.GetDirectoryName(filePath)))
        {
            if (filePath != string.Empty) Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? string.Empty);
        }
        
        try
        {
            File.WriteAllBytes(filePath, bytes);
            Debug.Log($"AR screenshot saved to: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write screenshot: {e.Message}");
        }
        
        Destroy(rt);
        Destroy(image);
    }

    private IEnumerator CaptureScreenshot()
    {
        foreach (var obj in objectsToDisable) obj.SetActive(false);
        
        yield return new WaitForEndOfFrame();
        
        var fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        var filePath = Path.Combine(screenshotManager.GetExportPath(), "screenshots", fileName);
        var tex = ScreenCapture.CaptureScreenshotAsTexture();

        if (tex == null)
        {
            Debug.Log("texture failed");
        }
        else
        {
            Debug.Log("texture created");
        }
        
        foreach (var obj in objectsToDisable) obj.SetActive(true);

        if (tex != null)
        {
            byte[] pngBytes = tex.EncodeToPNG();

            // Always destroy the in-memory texture if you’re done with it:
            Destroy(tex);

            try
            {
                File.WriteAllBytes(filePath, pngBytes);
                Debug.Log($"[ARScreenshot] Saved PNG to: {filePath}");
            }
            catch (IOException e)
            {
                Debug.LogError($"[ARScreenshot] Failed to write screenshot: {e.Message}");
            }
        }
        
        Debug.Log($"Screenshot queued: {filePath}");
    }
}