using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

[HelpURL("https://www.dropbox.com/s/u7a1brws02ttm21/RealSunAR%20Tester.pdf?dl=1")]
public class RealSunARTester : MonoBehaviour
{
    [HideInInspector]
    public string version = "1.10";

    [HideInInspector]
    public bool readingUpdate = false;

    [Tooltip("This is the camera whose transform will be used to check for camera rotation. This will also be used in the canvas during runtime.")]
    public Transform mainCamera;

    Text text;
    RectTransform headingNeedle;
    RectTransform cameraNeedle;

    float timerForCompassReading = 0.2f;
    float myCurrentCompass;
    Vector3 myInitialCameraRotation;

    bool cameraWorks = false;
    bool finalResult = false;

    void Awake()
    {
        text = GameObject.Find("RealSunAR Tester text").GetComponent<Text>();
        headingNeedle = GameObject.Find("Heading Dial Pivot").GetComponent<RectTransform>();
        cameraNeedle = GameObject.Find("Camera Dial Pivot").GetComponent<RectTransform>();

#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation) && !Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
#endif
        //Permisions were here
        //Caching the camera
        try
        {
            if (mainCamera == null) mainCamera = Camera.main.transform;
        }
        catch (Exception e)
        {
            text.text = "ERROR " + e.Message;
            text = null;
        }

        GetComponent<Canvas>().worldCamera = mainCamera.GetComponent<Camera>();

        //Enable gyro
        Input.gyro.enabled = true;
        //Enable GPS
        Input.location.Start(500f);
        //Enable compass
        Input.compass.enabled = true;
        Input.compensateSensors = true;
    }

    private void Start()
    {
        myInitialCameraRotation = mainCamera.rotation.eulerAngles;
    }

    void Update()
    {
        if (mainCamera.rotation.eulerAngles != myInitialCameraRotation) cameraWorks = true;

        if (timerForCompassReading < 0)
        {
            myCurrentCompass = Input.compass.trueHeading;
            timerForCompassReading = 0.2f;
            headingNeedle.localEulerAngles = new Vector3(headingNeedle.localRotation.eulerAngles.x, headingNeedle.localRotation.eulerAngles.y, -myCurrentCompass);
        }
        timerForCompassReading -= Time.unscaledDeltaTime;

        finalResult = (Input.gyro.enabled && Input.gyro.enabled && Input.location.isEnabledByUser && cameraWorks) ? true : false;
        string stringToShow = "Testing RealSunAR\n" +
                    "Platform: " + "<b>" + Application.platform.ToString() + "</b>\n" +
                    "-----------------------------------\n";
        if (Input.compass.enabled)
        {
            stringToShow = stringToShow + "Compass\t\t\t\t: PASSED with values... " + myCurrentCompass + "\n";
        }
        else
            stringToShow = stringToShow + "Compass\t\t\t\t: FAILED\n";
        if (Input.gyro.enabled)
            stringToShow = stringToShow + "GyroscopeAcc\t: PASSED with values... " + Input.gyro.userAcceleration.ToString() + "\n";
        else
            stringToShow = stringToShow + "GyroscopeAcc\t: FAILED\n";
        if (Input.location.isEnabledByUser)
            stringToShow = stringToShow + "GPS\t\t\t\t\t\t: PASSED --- Status " + Input.location.status.ToString() + "\n";
        else
            stringToShow = stringToShow + "GPS\t\t\t\t\t\t: FAILED\n";
        if (cameraWorks)
        {
            stringToShow = stringToShow + "Camera rotation\t: PASSED with values... " + mainCamera.rotation.eulerAngles.ToString() + "\n";
            cameraNeedle.localEulerAngles = new Vector3(cameraNeedle.localRotation.eulerAngles.x, cameraNeedle.localRotation.eulerAngles.y, -mainCamera.rotation.eulerAngles.y);
        }
        else
            stringToShow = stringToShow + "Camera rotation\t: FAILED\n";
        stringToShow = stringToShow     + "-----------------------------------\n";
        if (finalResult)
            stringToShow = stringToShow + "All tests passed. RealSunAR is compatible!";
        else
            stringToShow = stringToShow + "It seems there was a failure.\nPlease contact the developer at RealSunAR@gmail.com\nThank you for your time.";
        if (Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.OSXEditor ||
            Application.platform == RuntimePlatform.LinuxEditor)
            stringToShow = stringToShow + "\n\nWARNING!!! YOU NEED TO BUILD THIS ON THE TARGET DEVICE TO GET PROPER READINGS";

        //Passing string to the screen
        text.text = stringToShow;
    }

#if UNITY_EDITOR
    public void CheckUpdates()
    {
        readingUpdate = true;
        StartCoroutine(GetRequest());
        Debug.Log("<b>RealSunAR</b>L Checking for update...");
    }

    IEnumerator GetRequest()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://www.dropbox.com/s/35brn8vc67ey10f/RealSunAR%20Tester%20Version.txt?dl=1"))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.LogError("<b>RealSunAR</b>: There was a problem reaching the internet");
            }
            else
            {
                if (version == webRequest.downloadHandler.text)
                    Debug.Log("<b>RealSunAR</b>: You are already using the latest version!");
                else
                {
                    Debug.Log("<b>RealSunAR</b>: new version " + webRequest.downloadHandler.text + " found!\n" +
                              "get it here <b>http://bit.ly/3aKrYJH</b>");
                }
            }          
            readingUpdate = false;
        }
    }
#endif
}
    

