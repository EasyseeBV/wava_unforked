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
        //ScreenshotManager.onCaptureEndDelegate += OnCaptureEndDelegate;
        // Call update to only capture the texture without exporting
        //screenshotManager.UpdateAll();
    }

    private IEnumerator CaptureScreenshot()
    {
        foreach (var obj in objectsToDisable) obj.SetActive(false);
        
        yield return new WaitForEndOfFrame();
        
        var fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        var filePath = Path.Combine(screenshotManager.GetExportPath(), fileName);
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