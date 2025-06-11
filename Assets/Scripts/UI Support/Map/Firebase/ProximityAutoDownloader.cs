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

        foreach (var art in closestThree)
        {
            Debug.Log("Found art: " + art.title);
            DownloadManager.Instance.LoadModels(art.content_list, art.id);
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