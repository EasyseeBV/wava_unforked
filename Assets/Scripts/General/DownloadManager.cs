using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DownloadManager : MonoBehaviour
{
    private static DownloadManager _instance;
    public static DownloadManager Instance => _instance;
    
    private void Awake() 
    {
        if (_instance == null) 
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        } 
        else 
        {
            Destroy(gameObject);
        }
    }

    public async Task<(string localPath, bool downloaded)> BackgroundDownloadMedia(string storagePath, string path, ARDownloadBar downloadBar, int index = 0)
    {
        return await FirebaseLoader.DownloadContent(storagePath, path, downloadBar, index);
    }

    public async Task<(string localPath, bool downloaded)> BackgroundDownloadImage(string storagePath, string path,
        ARDownloadBar downloadBar, int index = 0)
    {
        return await FirebaseLoader.DownloadImage(storagePath, path, downloadBar, index);
    }
}