using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ARLoader : MonoBehaviour
{
    public static void Open(ArtworkData artwork, float _distance = 12)
    {
        ArTapper.ArtworkToPlace = artwork;
        ArTapper.PlaceDirectly = false; // old system?
        ArTapper.DistanceWhenActivated = _distance;
        
        // Sun position information
        SolarPositionAlgorithm.Latitude = (float)PlayerMarker.Instance.Latitude;
        SolarPositionAlgorithm.Longitude = (float)PlayerMarker.Instance.Longitude;

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
    
}
