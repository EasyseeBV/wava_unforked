using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DistanceValidator
{
    public static bool InRange(ArtworkData artwork)
    {
        var player = PlayerMarker.Instance;
        var distance = GeoUtils.DistanceInMeters(artwork.latitude, artwork.longitude, player.Latitude, player.Longitude);
        return distance <= 50f;
    }
}
