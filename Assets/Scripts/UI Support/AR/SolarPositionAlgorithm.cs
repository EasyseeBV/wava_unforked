using System;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class SolarPositionAlgorithm : MonoBehaviour
{
    private float latitude  = 0f;
    private float longitude = 0f;

    // Optional override from elsewhere in your code:
    public static float Latitude  = 0f;
    public static float Longitude = 0f;

    private Light dirLight;

    private void OnEnable()
    {
        dirLight = GetComponent<Light>();

        // If external static overrides are non-zero, use them;
        // otherwise pull from your ArtworkToPlace fallback
        latitude  = (Latitude  != 0f)
                  ? Latitude
                  : (float)ArTapper.ArtworkToPlace.latitude;
        longitude = (Longitude != 0f)
                  ? Longitude
                  : (float)ArTapper.ArtworkToPlace.longitude;
    }

    private void Start()
    {
        // 1) Compute current local time & offset
        DateTime utcNow    = DateTime.UtcNow;
        float    tzOffsetH = (float)DateTimeOffset.Now.Offset.TotalHours;
        DateTime localNow  = utcNow.AddHours(tzOffsetH);

        // 2) Sun angles
        (float elevation, float azimuth) = CalculateSun(localNow, latitude, longitude, tzOffsetH);

        // 3) Build the “toward‐sun” vector
        float elevRad = elevation * Mathf.Deg2Rad;
        float azRad   = azimuth   * Mathf.Deg2Rad;
        Vector3 sunDir = new Vector3(
            Mathf.Cos(elevRad) * Mathf.Sin(azRad),
            Mathf.Sin(elevRad),
            Mathf.Cos(elevRad) * Mathf.Cos(azRad)
        );

        // 4) Point the light *from* the Sun *onto* your scene
        transform.rotation = Quaternion.LookRotation(-sunDir, Vector3.up);
    }

    // Simplified NOAA‐style solar position
    private (float elevation, float azimuth)
    CalculateSun(DateTime date, float lat, float lon, float tzOffset)
    {
        // Julian Day & Century
        double jd = date.ToOADate() + 2415018.5;
        double jc = (jd - 2451545.0) / 36525.0;

        // Mean longitude & anomaly
        double L0 = (280.46646 + jc*(36000.76983 + jc*0.0003032)) % 360.0;
        double M  = 357.52911 + jc*(35999.05029 - 0.0001537*jc);

        // Equation of center
        double C = (1.914602 - jc*(0.004817 + 0.000014*jc))*Math.Sin(M*Mathf.Deg2Rad)
                 + (0.019993 - 0.000101*jc)*Math.Sin(2*M*Mathf.Deg2Rad)
                 + 0.000289*Math.Sin(3*M*Mathf.Deg2Rad);

        double sunTrueLong = L0 + C;
        double omega       = 125.04 - 1934.136*jc;
        double sunAppLong  = sunTrueLong - 0.00569 - 0.00478*Math.Sin(omega*Mathf.Deg2Rad);

        // Obliquity & declination
        double obliq0     = 23.0 + (26.0 + ((21.448
                            - jc*(46.815 + jc*(0.00059 - jc*0.001813)))/60.0))/60.0;
        double obliqCorr  = obliq0 + 0.00256*Math.Cos(omega*Mathf.Deg2Rad);
        double decl       = Math.Asin(Math.Sin(obliqCorr*Mathf.Deg2Rad)
                            * Math.Sin(sunAppLong*Mathf.Deg2Rad)) * Mathf.Rad2Deg;

        // Equation of time (minutes)
        double y = Math.Tan((obliqCorr/2)*Mathf.Deg2Rad);
               y *= y;
        double eqTime = 4.0 * Mathf.Rad2Deg * (
              y * Math.Sin(2*L0*Mathf.Deg2Rad)
            - 2*0.016708634 * Math.Sin(M*Mathf.Deg2Rad)
            + 4*0.016708634 * y * Math.Sin(M*Mathf.Deg2Rad) * Math.Cos(2*L0*Mathf.Deg2Rad)
            - 0.5*y*y * Math.Sin(4*L0*Mathf.Deg2Rad)
            - 1.25*0.016708634*0.016708634 * Math.Sin(2*M*Mathf.Deg2Rad)
        );

        // True Solar Time & Hour Angle
        double timeOffset  = eqTime + 4.0*lon - 60.0*tzOffset;
        double trueSolarMin= date.Hour*60 + date.Minute + date.Second/60.0 + timeOffset;
        double hourAngle   = (trueSolarMin/4.0) - 180.0;

        // Elevation
        double latRad      = lat * Mathf.Deg2Rad;
        double hrAngRad    = hourAngle * Mathf.Deg2Rad;
        double elev        = Math.Asin(
            Math.Sin(latRad)*Math.Sin(decl*Mathf.Deg2Rad) +
            Math.Cos(latRad)*Math.Cos(decl*Mathf.Deg2Rad)*Math.Cos(hrAngRad)
        ) * Mathf.Rad2Deg;

        // Azimuth
        double az = Math.Acos(
            (Math.Sin(latRad)*Math.Cos(elev*Mathf.Deg2Rad)
             - Math.Sin(decl*Mathf.Deg2Rad))
            / (Math.Cos(latRad)*Math.Sin(elev*Mathf.Deg2Rad))
        ) * Mathf.Rad2Deg;
        if (hourAngle > 0) az = 360 - az;

        return ((float)elev, (float)az);
    }
}
