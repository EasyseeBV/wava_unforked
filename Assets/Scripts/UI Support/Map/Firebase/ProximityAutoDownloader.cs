using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ProximityAutoDownloader : MonoBehaviour
{
    private void Start()
    {
        Invoke(nameof(Download), 1f);
    }

    private async Task Download()
    {
        float userLon, userLat;
        OnlineMapsLocationServiceBase.baseInstance.GetLocation(out userLon, out userLat);

        var closestThree = FirebaseLoader.Artworks
            .OrderBy(a => HaversineDistance(userLat, userLon, a.latitude, a.longitude))
            .Take(3);

        Debug.Log("count found: " + closestThree.Count());
        foreach (var art in closestThree)
        {
            Debug.Log("Found art: " + art.title);
            foreach (var content in art.content_list)
            {
                var uri = new Uri(content.media_content);
                string encodedPath = uri.AbsolutePath;
                string decodedPath = Uri.UnescapeDataString(encodedPath);
                string fileName = Path.GetFileName(decodedPath);

                string path = content.media_content; // default the path to the firestore uri of the content
                string localPath = Path.Combine(AppCache.ContentFolder, fileName);
            
                // if the file does not exist locally, download it
                if (!File.Exists(localPath))
                {
                    // failed to download handling needs to be done here
                    if (FirebaseLoader.OfflineMode)
                    {
                        Debug.Log("OfflineMode");
                        return;
                    }

                    var results = await DownloadManager.Instance.BackgroundDownloadMedia(AppCache.ContentFolder, content.media_content, null);
                    path = results.localPath;
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    {
                        Debug.Log("Content was downloaded and stored locally");
                    }
                }
                else if (File.Exists(localPath)) // if the file does exist, set the path to that location
                {
                    Debug.Log("Content was already found locally");
                }
            }
        }
    }
    
    private double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // Earth radius in meters
        double dy = lat1 * Mathf.Deg2Rad;
        double dx = lat2 * Mathf.Deg2Rad;
        double fy = (lat2 - lat1) * Mathf.Deg2Rad;
        double fx = (lon2 - lon1) * Mathf.Deg2Rad;

        double a = Math.Sin(fy/2) * Math.Sin(fy/2) +
                   Math.Cos(dy) * Math.Cos(dx) *
                   Math.Sin(fx/2) * Math.Sin(fx/2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }
}