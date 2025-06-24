using System;

public static class GeoUtils
{
    // Earth’s radius in meters
    private const double EarthRadius = 6371000;

    /// <summary>
    /// Returns the distance between two lat/lon points in meters.
    /// </summary>
    public static double DistanceInMeters(double lat1, double lon1, double lat2, double lon2)
    {
        // Convert degrees to radians
        double φ1 = lat1 * Math.PI / 180.0;
        double φ2 = lat2 * Math.PI / 180.0;
        double Δφ = (lat2 - lat1) * Math.PI / 180.0;
        double Δλ = (lon2 - lon1) * Math.PI / 180.0;

        double a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                   Math.Cos(φ1) * Math.Cos(φ2) *
                   Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadius * c;
    }
}
