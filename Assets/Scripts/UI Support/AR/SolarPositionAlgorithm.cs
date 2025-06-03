using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class SolarPositionAlgorithm : MonoBehaviour
{
    private Light _dirLight;

    // GPS / Location
    private bool    _locationReady = false;
    private double  _latitude     = 0.0;
    private double  _longitude    = 0.0;

    // Compass
    private bool  _compassReady = false;
    private float _trueHeading  = 0f; // degrees clockwise from true north

    // How often to update (in seconds)
    private const float _updateInterval = 60f;
    private float _timeSinceLastUpdate = 0f;

    private void Awake()
    {
        _dirLight = GetComponent<Light>();
        if (_dirLight.type != LightType.Directional)
        {
            Debug.LogWarning("[ARSunAlignedLight] Switching Light to Directional mode.");
            _dirLight.type = LightType.Directional;
        }
    }

    private void Start()
    {
        StartCoroutine(InitializeSensors());
    }

    private IEnumerator InitializeSensors()
    {
        // 1) LOCATION setup
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("[ARSunAlignedLight] Location services not enabled by user.");
        }
        else
        {
            Input.location.Start();
            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }

            if (maxWait <= 0 || Input.location.status == LocationServiceStatus.Failed)
            {
                Debug.LogWarning("[ARSunAlignedLight] Failed to initialize GPS.");
            }
            else
            {
                // First fix
                _latitude  = Input.location.lastData.latitude;
                _longitude = Input.location.lastData.longitude;
                _locationReady = true;
            }
        }

        // 2) COMPASS setup
        if (!SystemInfo.supportsGyroscope && !SystemInfo.supportsAccelerometer)
        {
            Debug.LogWarning("[ARSunAlignedLight] Device has no magnetic sensor / accelerometer. Compass unavailable.");
        }
        else
        {
            Input.compass.enabled = true;
            Input.gyro.enabled    = true; // enabling gyro helps Unity fuse accelerometer + magnetometer for a better heading
            // Wait until compass has a valid reading (timestamp > 0)
            float compassTimeout = 5f;
            while (Input.compass.timestamp == 0 && compassTimeout > 0f)
            {
                yield return new WaitForSeconds(0.2f);
                compassTimeout -= 0.2f;
            }

            if (Input.compass.timestamp == 0)
            {
                Debug.LogWarning("[ARSunAlignedLight] Compass failed to initialize.");
            }
            else
            {
                _trueHeading   = Input.compass.trueHeading;
                _compassReady  = true;
            }
        }

        // If both are ready, do an initial sunlight update now
        if (_locationReady && _compassReady)
            UpdateSunlightDirection();
    }

    private void Update()
    {
        if (!_locationReady || !_compassReady)
            return;

        _timeSinceLastUpdate += Time.deltaTime;
        if (_timeSinceLastUpdate >= _updateInterval)
        {
            // Refresh latitude/longitude & compass heading each interval
            _latitude   = Input.location.lastData.latitude;
            _longitude  = Input.location.lastData.longitude;
            _trueHeading = Input.compass.trueHeading;

            UpdateSunlightDirection();
            _timeSinceLastUpdate = 0f;
        }
    }

    private void UpdateSunlightDirection()
    {
        // 1) TIME / DATE
        DateTime nowLocal = DateTime.Now;
        DateTime nowUtc = nowLocal.ToUniversalTime();
        int dayOfYear = nowUtc.DayOfYear;
        double hour = nowLocal.Hour + nowLocal.Minute / 60.0 + nowLocal.Second / 3600.0;

        // 2) FRACTIONAL YEAR (γ) for sun formulas
        double gamma = 2.0 * Math.PI / 365.0 * (dayOfYear - 1 + (hour - 12.0) / 24.0);

        // 3) EQUATION OF TIME (in minutes)
        double eqTime = 229.18 * (
            0.000075
            + 0.001868 * Math.Cos(gamma)
            - 0.032077 * Math.Sin(gamma)
            - 0.014615 * Math.Cos(2 * gamma)
            - 0.040849 * Math.Sin(2 * gamma)
        );

        // 4) SOLAR DECLINATION (δ, in radians)
        double decl = 0.006918
                      - 0.399912 * Math.Cos(gamma)
                      + 0.070257 * Math.Sin(gamma)
                      - 0.006758 * Math.Cos(2 * gamma)
                      + 0.000907 * Math.Sin(2 * gamma)
                      - 0.002697 * Math.Cos(3 * gamma)
                      + 0.00148 * Math.Sin(3 * gamma);

        // 5) TIME OFFSET in minutes: eqTime + 4*Longitude − 60*TimezoneOffset
        double tzOffsetHours = TimeZoneInfo.Local.GetUtcOffset(nowLocal).TotalHours;
        double timeOffset = eqTime + 4.0 * _longitude - 60.0 * tzOffsetHours;

        // 6) TRUE SOLAR TIME (TST, in minutes)
        double tst = hour * 60.0 + timeOffset;

        // 7) HOUR ANGLE (H, in degrees): H = (TST / 4) − 180
        double hourAngleDeg = (tst / 4.0) - 180.0;
        double hourAngleRad = hourAngleDeg * Mathf.Deg2Rad;

        // 8) LATITUDE in radians
        double latRad = _latitude * Mathf.Deg2Rad;

        // 9) COS(θ) = sin(lat)*sin(decl) + cos(lat)*cos(decl)*cos(H)
        double cosZenith = Math.Sin(latRad) * Math.Sin(decl) +
                           Math.Cos(latRad) * Math.Cos(decl) * Math.Cos(hourAngleRad);
        cosZenith = Mathf.Clamp((float)cosZenith, -1f, 1f);
        double zenithRad = Math.Acos(cosZenith);

        // 10) ELEVATION = 90° − θ
        double elevationRad = (Math.PI / 2.0) - zenithRad;

        // 11) RAW AZIMUTH (from due north, clockwise)
        //      az = atan2( sin(H), cos(H)*sin(lat) − tan(decl)*cos(lat) )
        double azRad = Math.Atan2(
            Math.Sin(hourAngleRad),
            Math.Cos(hourAngleRad) * Math.Sin(latRad) - Math.Tan(decl) * Math.Cos(latRad)
        );
        // Convert to degrees 0–360, where 0 = North.
        double sunAzimuthDeg = azRad * Mathf.Rad2Deg + 180.0;

        // 12) COMPASS HEADING ADJUSTMENT
        //   Input.compass.trueHeading is degrees clockwise from true-north to “device’s forward direction in world-space”.
        //   We want an azimuth relative to the AR world’s +Z axis. If the phone’s +Z was pointing  𝜃 degrees clockwise from north at startup,
        //   then the sun’s azimuth relative to AR-Z is simply (sunAzimuth − trueHeading).
        double relativeAzimuthDeg = sunAzimuthDeg - _trueHeading;
        // Normalize to [0,360)
        relativeAzimuthDeg = (relativeAzimuthDeg + 360.0) % 360.0;
        double relativeAzimuthRad = relativeAzimuthDeg * Mathf.Deg2Rad;

        // 13) BUILD 3D SUN DIRECTION in AR-WORLD COORDS
        //   - In “North/East/Up” coords, 
        //       x_NEU = cos(elev)*sin(az)   (east component)
        //       y_NEU = sin(elev)           (up component)
        //       z_NEU = cos(elev)*cos(az)   (north component)
        //   - But our AR world: 
        //       +Z = forward (device’s +Z at startup)  ≈ “North” once we subtract _trueHeading  
        //       +X = right (device’s +X at startup)    ≈ “East”
        //       +Y = up
        //
        //   So we can treat “az” here as measured from world-Z (north) toward world-X (east).
        double x = Math.Cos(elevationRad) * Math.Sin(relativeAzimuthRad);
        double y = Math.Sin(elevationRad);
        double z = Math.Cos(elevationRad) * Math.Cos(relativeAzimuthRad);
        Vector3 sunDir = new Vector3((float)x, (float)y, (float)z);

        // 14) APPLY to Directional Light (light “points” along its forward)
        _dirLight.transform.rotation = Quaternion.LookRotation(sunDir);
    }
}
