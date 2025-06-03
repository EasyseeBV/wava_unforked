//Uncomment the next line by removing the "//" symbols for debugging info on screen
//#define debuggingON
using System;                   //for DateTime & for string to int & Math functions
using System.Xml;               //for xml reading
using UnityEngine.Networking;   //for weather API reading
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

[RequireComponent(typeof(Light))]
[HelpURL("https://www.dropbox.com/s/auocy951vspv7sj/Manual.pdf?dl=1")]
public class RealSunAR : MonoBehaviour
{
    //VERSION 2.10
    
    //For singleton design
    public static RealSunAR singleDesign;

    [Tooltip("How many seconds should the device not move around before RealSunAR's calculations execute?")]
    [Range(1, 5)]
    public float stabilizedSecondsNeeded = 1;
    
    [Tooltip("How often should the engine try to detect if there was a compass drift?")]
    public float compassDriftTimer = 10;

    public enum compassQuality { Simple, Superb};
    [Tooltip("Which measuring method should be used for the NorthOffset-to-Cam calculations?")]
    public compassQuality compassReadout = compassQuality.Superb;
 
    [Tooltip("How much compass drift (in degrees) should be tolerated?")]
    [Range(0, 360)]
    public float tolerateDrift = 15f;

    [Tooltip("How many seconds should pass for each recalculation of Sun's time orbit?")]
    public int recalculateSeconds = 60;

    [Tooltip("Automaticaly sets the sun light to the optimal settings during runtime")]
    public bool optimizeSun = true;

    [Tooltip("Time to wait for GPS lock (in seconds)")]
    public int lockGPSWait = 20;

    [Tooltip("Keep GPS service alive even after GPS location is found?")]
    public bool keepGPSServiceAlive = false;

    [Tooltip("Should this plug in give out console warnings/errors?")]
    public bool enableConsoleWarnings = true;

    [Tooltip("For debugging: Phone vibrates when Sun is positioned")]
    public bool vibrateOnExecute = false;

    [Tooltip("Optional: Display the amount of cloudiness")]
    public UnityEngine.UI.Text cloudinessText;

    //Inspector values from here on can be hidden or exposed or control others in accordance to user choices

    //Interpolation public variables
    [Tooltip("Will the sun's intensity diminish as it goes near horizon level?")]
    public bool intensityInterpolation = true;
    [Tooltip("Will the sun's color change as it goes near horizon level?")]
    public bool colorInterpolation = true;
    [HideInInspector]
    public float anglesWhichInterpolate = 15f;
    [HideInInspector]
    public Color perpendicularSunColor = new Color(1f, 1f, 1f);
    [HideInInspector]
    public Color grazedSunColor = new Color(0.64f, 0f, 1f);
    
    [HideInInspector]
    public GameObject locationCanvas;    
    [HideInInspector]
    public int locationCanvasDuration = 5;

    public enum enumWeatherAPI { none, OpenWeatherMap };
    [HideInInspector]
    public enumWeatherAPI myWeather = enumWeatherAPI.none;
    [HideInInspector]
    [Tooltip("Please type your weather ID here")]
    public String weatherAPI_ID;
    [HideInInspector]
    public float shadowMIN = 0.1f;
    [HideInInspector]
    public float shadowMAX = 0.7f;

    //This will hold the RealSunAR's parent for the DualAxis System to function
    Transform RealSunParent;

    //Variable to cache the main camera
    Transform mainCamera;

    Light sun;

    bool gotGPSlock = false;

    //Hold default gameobject rotation to be used if GPS or Compass fails
    private Vector3 originalRotation;

    //variables that control the whole engine functionality
    [HideInInspector]
    public bool killSwitch = false;
    [HideInInspector]
    public bool hasInitialized = false;

    Vector3 camTilt; //Used for code cleanup
    float timerForActivision = 0f;
    float timerForCompassDriftDetection = 0f;
    float timerForOrbitRecalc = 0f;
    
    //Variables that hold data for orbit calculations
    float GPSLatitude;
    float GPSLongtitude;
    float positionSeasonSun = 0f; //this will used for the earth Summer/Winter tilt
    float positionTimeSun = 0f;   //this will be used for the 24hour rotation around the earth

#if debuggingON
    //DEBUG variables for debugging & data logging
    //IMPORTANT: Remember to change android player settings -> write permission to SDcard to make the logger work
    //Hidden bool that activates the data logger
    bool dataLogger = false;
    string rawData = "";
    string dataToLog = "";
    int dataLoggerCount = 1;
    float helperForCompass = 0.33f;
    float myDebugCompass;
    float myDebugPreviousNorthOffset;
    float myDebugLastReadout;
    float myDebugDrift;
    int myDebugReadouts = 0;
    DateTime debugTime;
#endif

    //Compass drift engine variables
    List<float> measuringCompass;
    float previousNorthOffset;

    void Awake()
    {
        if (RealSunAR.singleDesign != null)
            Destroy(gameObject);
        else RealSunAR.singleDesign = this;

#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation) && !Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.CoarseLocation);
        }
#endif

        //Caching the camera
        mainCamera = Camera.main.transform;

        //I'm pretty sure the AR SDK you are using has already turned this on but I place it here just in case for other usages
        Input.gyro.enabled = true;
        if (weatherAPI_ID == "") myWeather = enumWeatherAPI.none;

        //set directional light
        sun = GetComponent<Light>();

        //Make sure that if color interpolation is on, the sun's color is the one set on RealSun's inspector
        if (colorInterpolation) sun.color = perpendicularSunColor;

        //Sets the light to optimal settings
        if (optimizeSun)
        {
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            sun.shadowResolution = UnityEngine.Rendering.LightShadowResolution.VeryHigh;
        }

        //Keep the transform the developer original created in case everything fails
        originalRotation = transform.rotation.eulerAngles;

        //Enable GPS
        Input.location.Start(500f);
        //Enable compass
        Input.compass.enabled = true;
        Input.compensateSensors = true;

        measuringCompass = new List<float>();

        //Simple trick just to make the compass get starting!
        float nullVariable = Input.compass.trueHeading;

#if debuggingON
        //Datalogger works best with the superb engine
        if (dataLogger)
        {
            compassReadout = compassQuality.Superb;
            System.IO.Directory.CreateDirectory("/sdcard/RealSun/");
        }
#endif
        }

    IEnumerator Start()
    {
        RealSunParent = new GameObject("RealSunParent").transform;
        RealSunParent.parent = this.transform.parent;
        this.transform.parent = RealSunParent.transform;

        StartCoroutine("GetGPS");
        StartCoroutine("PositionSeasonSun");
        StartCoroutine("GetRequest");

        //Simple trick just to make the compass get starting!
        float nullVariable = Input.compass.trueHeading;

        yield return null;
    }

    void Update()
    {
        //DEBUG
        //if (Input.GetMouseButtonDown(0)) hasInitialized = false;

#if debuggingON
        if (helperForCompass < 0)
        {
            myDebugCompass = Input.compass.trueHeading;
            helperForCompass = 0.33f;
        }
        else helperForCompass -= Time.unscaledDeltaTime;
#endif

        if (!killSwitch && gotGPSlock)
        {

            camTilt = mainCamera.rotation.eulerAngles;
            
            if (!hasInitialized)
            {
                //Execute calculations if phone has been rather still the last "stabilizedSecondsNeeded" seconds and within 20 degrees side tilt and 140 degrees vertical tilt
                if ((camTilt.x > 10f && camTilt.x < 90f) && (camTilt.z > 350f || camTilt.z < 10f) &&
                    Input.gyro.userAcceleration.x < 0.15f && Input.gyro.userAcceleration.y < 0.15f && Input.gyro.userAcceleration.z < 0.15f)
                {
                    if (timerForActivision > stabilizedSecondsNeeded)
                    {

#if debuggingON
                        if (dataLogger)
                        {
                            string fileName = "/sdcard/RealSun/RawData" + dataLoggerCount + ".txt";
                            System.IO.File.AppendAllText(fileName, rawData);
                        }
#endif

                        float myOffset = (compassReadout == compassQuality.Superb) ? CameraToCompassAverage(measuringCompass) : mainCamera.rotation.eulerAngles.y - Input.compass.trueHeading;
                        PositionSun(myOffset);

#if debuggingON
                        myDebugPreviousNorthOffset = previousNorthOffset;
                        myDebugLastReadout = previousNorthOffset;
                        //***myDebugDrift = 0;
#endif

                        hasInitialized = true;

                        timerForActivision = 0;
                        timerForCompassDriftDetection = 0;
                        timerForOrbitRecalc = 0;
                        measuringCompass.Clear();

#if debuggingON
                        rawData = "";
#endif

                    }
                    else
                    {
                        timerForActivision += Time.unscaledDeltaTime;
                        if (timerForActivision / stabilizedSecondsNeeded > 0.5f && compassReadout == compassQuality.Superb)
                        {
                            //Gathering data to pull average initial North offset readout
                            measuringCompass.Add(mainCamera.rotation.eulerAngles.y - Input.compass.trueHeading);

#if debuggingON
                            if (dataLogger) rawData += mainCamera.rotation.eulerAngles.y + " " + Input.compass.trueHeading + "\n";
                            myDebugReadouts = measuringCompass.Count;
#endif

                        }
                    }
                }
                else
                {
                    timerForActivision = 0f;
                    measuringCompass.Clear();

#if debuggingON
                    rawData = "";
                    myDebugReadouts = 0;
#endif

                }
            }
            else
            {
                if (timerForCompassDriftDetection > compassDriftTimer)
                {
                    //Gather new data
                    if ((camTilt.x > 10f && camTilt.x < 90f) && (camTilt.z > 350f || camTilt.z < 10f) &&
                    Input.gyro.userAcceleration.x < 0.15f && Input.gyro.userAcceleration.y < 0.15f && Input.gyro.userAcceleration.z < 0.15f)
                    {
                        if (timerForActivision > stabilizedSecondsNeeded)
                        {

#if debuggingON
                            if (dataLogger)
                            {
                                string fileName = "/sdcard/RealSun/RawData" + dataLoggerCount + ".txt";
                                System.IO.File.AppendAllText(fileName, rawData);
                            }
#endif

                            float myOffset = (compassReadout == compassQuality.Superb) ? CameraToCompassAverage(measuringCompass) : mainCamera.rotation.eulerAngles.y - Input.compass.trueHeading;
                            float myCurrentDrift = CompareAngles(myOffset);

                            //Check to see if there was a big compass drift or not and if there was, use the new readout
                            if (myCurrentDrift > tolerateDrift)
                            {
                                PositionSun(myOffset);
                                timerForOrbitRecalc = 0f;
                            }
                            timerForActivision = 0f;
                            timerForCompassDriftDetection = 0f;
                            measuringCompass.Clear();

#if debuggingON
                            rawData = "";
#endif

                        }
                        else
                        {
                            timerForActivision += Time.unscaledDeltaTime;
                            if (timerForActivision / stabilizedSecondsNeeded > 0.5f && compassReadout == compassQuality.Superb)
                            {
                                //Gathering data to pull average compassdrift
                                measuringCompass.Add(mainCamera.rotation.eulerAngles.y - Input.compass.trueHeading);

#if debuggingON
                                if (dataLogger) rawData += mainCamera.rotation.eulerAngles.y + " " + Input.compass.trueHeading + "\n";
                                myDebugReadouts = measuringCompass.Count;
#endif

                            }
                        }
                    }
                    else
                    {
                        timerForActivision = 0f;
                        measuringCompass.Clear();

#if debuggingON
                        rawData = "";
                        myDebugReadouts = 0;
#endif

                    }
                }
                timerForCompassDriftDetection += Time.unscaledDeltaTime;

                if (timerForOrbitRecalc > recalculateSeconds)
                {
                    TimedPositionSun();
                    timerForOrbitRecalc = 0f;
                }
                timerForOrbitRecalc += Time.unscaledDeltaTime;
            }
        }
        else
        {
            timerForCompassDriftDetection = 0f;
            timerForActivision = 0f;
        }
    }

#if debuggingON
    private void OnGUI()
    {
            GUIStyle myStyle = new GUIStyle();
            Texture2D tex = new Texture2D(2, 2);
            myStyle.normal.textColor = Color.white;
            myStyle.fontSize = 45;
            myStyle.normal.background = tex;
            tex = SetColor(tex, new Color32(0, 0, 0, 100));
            GUILayout.BeginVertical();

            string myString = 
            "Debugging info\n" +
            "---------------------\n" +
            "Camera Rotation\t: "+ camTilt + "\n" +
            "GPS \t\t" + Input.location.status.ToString() + "\n" +
            "Compass        \t: " + myDebugCompass.ToString("0") + "\n" +
            "PreviousNorthOffset\t: " + myDebugPreviousNorthOffset + "\n" +
            "LastNorthOffset\t: " + myDebugLastReadout + "\n" +
            "Drift\t\t: " + myDebugDrift + "\n" +
            "ForActivision  \t: " + timerForActivision.ToString("0.0") + "\n" +
            "ForCompassDrn  \t: " + timerForCompassDriftDetection.ToString("0.0") + "\n" +
            "ForReOrbitSec \t: " + timerForOrbitRecalc.ToString("0.0") + "\n" +
            "Readouts Collectd\t: " + myDebugReadouts + "\n" +
            "Logfile number    \t: " + (dataLoggerCount - 1) + "\n" +
            "Tolerate Drift    \t: " + tolerateDrift.ToString() + "\n" +
            "Compass mode   \t: " + compassReadout.ToString() + "\n" +
            "Latitude: " + GPSLatitude + " Longtitude: " + GPSLongtitude + "\n" +
            "DateTime: " + debugTime.ToString() + "\n" +            
            "Kill= " + killSwitch + " GPS = " + gotGPSlock + " hasInit = " + hasInitialized;
            
            GUILayout.Label(myString, myStyle, GUILayout.ExpandHeight(true));
            GUILayout.EndVertical();
    }
#endif

void PositionSun(float myOffset)
    {
        if (gotGPSlock && Input.compass.enabled == true)
        {
            //Reseter();
            previousNorthOffset = myOffset;
            hasInitialized = true;

            //GPSLong - GPSLat - Summer/Winter - Time - Bearing

            //Reset rotation
            this.gameObject.transform.rotation = Quaternion.Euler(Vector3.zero);

            //Apply fix - GPSLatitude + Earth's tilt + Compass (to parent) also note down the NorthOffset
            RealSunParent.rotation = Quaternion.Euler(90f - GPSLatitude + positionSeasonSun, myOffset, 0f);            
         
            //Apply GPS GPSLatitude (to self)
            transform.localRotation = Quaternion.Euler(0f, GPSLongtitude + positionTimeSun, 0f);

            //Apply color & intensity interpolation
            if (colorInterpolation || intensityInterpolation)
            {
                float myLerp = Mathf.InverseLerp(0f, anglesWhichInterpolate, gameObject.transform.rotation.eulerAngles.x);
                
                if (intensityInterpolation)
                {
                    float newIntensity = myLerp;
                    if (transform.rotation.eulerAngles.x > 90f) newIntensity = 0f;
                    sun.intensity = newIntensity;
                }
                if (colorInterpolation)
                {
                    sun.color = (gameObject.transform.rotation.eulerAngles.x > 90f) ? grazedSunColor : Color.Lerp(grazedSunColor, perpendicularSunColor, myLerp);
                }
            }

            if (vibrateOnExecute) Invoke("Vibrate",0.1f);
        }
    }

    public void Reseter()
    {
        //Might be used in the future
        //this.transform.parent = this.transform.parent.parent;
        //Destroy(RealSunParent.gameObject);

        hasInitialized = false;
        RealSunParent.rotation = Quaternion.Euler(Vector3.zero);
        this.gameObject.transform.rotation = Quaternion.Euler(originalRotation);
    }

    void TimedPositionSun()
    {
        DateTime nowDate = DateTime.Now.ToUniversalTime(); //Comment this out if you want to DEBUG (look below) 
        //DEBUG specific nowDate instead of current nowDate but remember to apply the same fixed time on PositionSeasonSun()
        //DateTime nowDate = new DateTime(2019, 9, 23, 7, 50, 0);

#if debuggingON
        debugTime = nowDate;
#endif

        //Calculate Time rotation
        double totalSeconds = (double)nowDate.Second + ((double)nowDate.Minute * 60) + ((double)nowDate.Hour * 3600);
        double result = ((totalSeconds / 86400) * 360) + 180;
        positionTimeSun = (float)result;

        //Apply GPS GPSLatitude (to self)
        transform.localRotation = Quaternion.Euler(0f, GPSLongtitude + positionTimeSun, 0f);
    }

    IEnumerator GetGPS()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            if (locationCanvas != null)
            {
                locationCanvas.SetActive(true);
                Invoke("KillCanvas", locationCanvasDuration);
            }
            yield break;
        }

        // Wait until service initializes
        while (Input.location.status == LocationServiceStatus.Initializing && lockGPSWait > 0)
        {
            yield return new WaitForSeconds(1);
            lockGPSWait--;
        }

        // Service didn't initialize in lockGPSWait seconds
        if (lockGPSWait < 1)
        {
            if (!keepGPSServiceAlive) Input.location.Stop();
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            if (!keepGPSServiceAlive) Input.location.Stop();
            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
            GPSLatitude = Input.location.lastData.latitude;
            GPSLongtitude = Input.location.lastData.longitude;

            //Delay activision of gotGPSlock until camera unfreezes
            do
            {
                yield return new WaitForEndOfFrame();
                //Simple trick just to make the compass get starting!
                float nullVariable = Input.compass.trueHeading;
            }
            while (mainCamera.rotation.eulerAngles == Vector3.zero);

            gotGPSlock = true;
            
        }
        // Stop service if there is no need to query location updates continuously
        if (!keepGPSServiceAlive) Input.location.Stop();        
        yield break;
    }

    IEnumerator PositionSeasonSun()
    {
        DateTime winterSolstace = new DateTime(2018, 12, 21, 22, 23, 0); //This is used as a constant variable
        const double longestTilt = 23.43679d;
        const double solsticeSeconds = 15778437.5d;

        DateTime nowDate = DateTime.Now.ToUniversalTime(); //Comment out if you want to DEBUG (look below) 
        //DEBUG specific nowDate instead of current nowDate but remember to apply the same fixed time on TimedPositionSun()
        //DateTime nowDate = new DateTime(2019, 9, 23, 7, 50, 0);

#if debuggingON
        debugTime = nowDate;
#endif

        TimeSpan timePassed = nowDate - winterSolstace;
        //Debug.Log("Day" + timePassed.Days);
        //Debug.Log("Hour" + timePassed.Hours);
        //Debug.Log("Minute" + timePassed.Minutes);
        //Debug.Log("Second" + timePassed.Seconds);

        int Seconds = timePassed.Seconds + (timePassed.Minutes * 60) + (timePassed.Hours * 3600) + (timePassed.Days * 86400);

        //Calculate Earth's tilt
        double result = longestTilt * Math.Sin((((double)Seconds / solsticeSeconds) * Math.PI) - (Math.PI / 2));
        positionSeasonSun = (float)result;
        //DEBUG
        //SummerTilt.text = "SummerTilt: " + positionSeasonSun.ToString();

        //Calculate Time rotation
        double totalSeconds = (double)nowDate.Second + ((double)nowDate.Minute * 60) + ((double)nowDate.Hour * 3600);
        result = ((totalSeconds / 86400) * 360) + 180;
        positionTimeSun = (float)result;
        //DEBUG
        //TimeLong.text = "Time position: " + positionTimeSun.ToString();
        yield break;
    }
       
    IEnumerator GetRequest()
    {
        XmlDocument xmlData;
        xmlData = new XmlDocument();
        string url = "";
        switch (myWeather)
        {
            case enumWeatherAPI.OpenWeatherMap:
                url = "https://api.openweathermap.org/data/2.5/weather?lat=" + GPSLatitude.ToString() + "&lon=" + GPSLongtitude.ToString() + "&mode=xml&APPID=" + weatherAPI_ID;
                //Debug.Log("URL: " + url);
                break;
            case enumWeatherAPI.none:
                StopCoroutine("GetRequest");
                yield return null;
                break;
        }

        while (!gotGPSlock)
        {
            yield return new WaitForEndOfFrame(); // just wait here till we sort out where we are!       
        }

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                //Debug.Log(" Error: " + webRequest.error);
            }
            else
            {
                xmlData.LoadXml(webRequest.downloadHandler.text);
                int cloudiness = Int32.Parse(xmlData.SelectSingleNode("current/clouds/@value").InnerText);
                if (cloudiness > -1 && cloudiness < 101)
                sun.shadowStrength = Mathf.Lerp(shadowMAX, shadowMIN, ((float)cloudiness / 100f));
                //Debug.Log("Cloudiness =" + cloudiness);
                if (cloudinessText != null) cloudinessText.text = "Cloudiness = " + cloudiness + "%";
            }
        }
    }

    //This Will take a listing of -360 to 360 range, find the median average, throw the top & last 20% peak values, find the average and return it on a 0-360 range
    //You will find a simple to understand model here: https://github.com/synthercat/Algorithm-Smoothing-Angles-in-Unity
    float CameraToCompassAverage(List<float> values)
    {
        int limit = values.Count;
        int[] Quadrants = new int[8];

#if debuggingON
        //Data logger
        string fileName = "/sdcard/RealSun/DataLog" + dataLoggerCount + ".txt";
        if (dataLogger)
        {        
            dataToLog = "";
            foreach (float value in values)
            {
                dataToLog += value.ToString() + "\n";
            }

            //DEBUG
            //texter5.text = "DATA SAVED: " + dataLoggerCount;

        }
#endif

        //Clip to 0-360 & find fill the array in order to find the dominant Quadrant
        for (int i = 0; i < limit; i++)
        {
            if (values[i] < 0) values[i] += 360f;

            if (values[i] >= 315f || values[i] <  45f) Quadrants[4]++;
            if (values[i] <  90f)                      Quadrants[3]++;
            if (values[i] >=  45f && values[i] < 135f) Quadrants[2]++;
            if (values[i] >=  90f && values[i] < 180f) Quadrants[1]++;
            if (values[i] >= 135f && values[i] < 225f) Quadrants[0]++;
            if (values[i] >= 180f && values[i] < 270f) Quadrants[7]++;
            if (values[i] >= 225f && values[i] < 315f) Quadrants[6]++;
            if (values[i] >= 270f)                     Quadrants[5]++;
        }

        //Measure the array to find the dominant Quadrant
        int dominantQuadrant = 0;
        int maxValue = Quadrants[0];
        for (int i = 1; i < 8; i++)
        {
            if (Quadrants[i] > maxValue)
            {
                maxValue = Quadrants[i];
                dominantQuadrant = i;
            }
        }

        //Offset the whole list to shift it torwards 180degrees
        for (int i = 0; i < limit; i++)
        {
            values[i] += 45f * dominantQuadrant;
            if (values[i] > 360) values[i] -= 360f;
        }

        //Trim the first and last 20% of the values to avoid spikes of bad compass readouts
        values.Sort();
        int trimmer = values.Count / 5;
        values = values.GetRange(trimmer, trimmer * 3);

        //Find the average and shift it back to it's original angle position
        int counterForAverage = 0;
        float sumForAverage = 0f;

        foreach (float value in values)
        {
            counterForAverage++;
            sumForAverage += value;
        }

        sumForAverage /= counterForAverage;
        sumForAverage -= 45f * dominantQuadrant;
        sumForAverage = (sumForAverage < 0) ? sumForAverage + 360f : sumForAverage;

#if debuggingON
        //Data logger
        if (dataLogger)
        {
            dataToLog += "Returned: " + sumForAverage.ToString();
            System.IO.File.AppendAllText(fileName, dataToLog);
            dataLoggerCount++;
        }
#endif

        return sumForAverage;
    }

    void Vibrate()
    {
        Handheld.Vibrate();
    }

    void KillCanvas()
    {
        locationCanvas.SetActive(false);
    }

    public void SetColor(Color newColor, float intensity)
    {
        sun.color = newColor;
        sun.intensity = intensity;
    }

    public void SetColor(Color newColor)
    {
        sun.color = newColor;
    }

    public void SetColor(float intensity)
    {
        sun.intensity = intensity;
    }

    float CompareAngles(float newOffset)
    {
        if (previousNorthOffset < 0) previousNorthOffset += 360f;
        if (newOffset < 0) newOffset += 360f;
        float drift = (Mathf.Max(previousNorthOffset, newOffset) - Mathf.Min(previousNorthOffset, newOffset));
        drift = (drift > 180f) ? 360f - drift : drift;

#if debuggingON
        myDebugPreviousNorthOffset = previousNorthOffset;
        myDebugLastReadout = newOffset;
        myDebugDrift = drift;
#endif

        return drift;
    }

#if debuggingON
    Texture2D SetColor(Texture2D tex2, Color32 color)
    {


        var fillColorArray = tex2.GetPixels32();

        for (var i = 0; i < fillColorArray.Length; ++i)
        {
            fillColorArray[i] = color;
        }

        tex2.SetPixels32(fillColorArray);

        tex2.Apply();

        return tex2;
    }
#endif

}