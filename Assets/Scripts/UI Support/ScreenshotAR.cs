using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AlmostEngine.Screenshot;
using UnityEngine;

public class ScreenshotAR : MonoBehaviour
{
    [SerializeField] private ScreenshotManager screenshotManager;

    public void Capture()
    {
        Handheld.Vibrate();
        StartCoroutine(CaptureAndSave());
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
}