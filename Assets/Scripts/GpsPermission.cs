using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class GpsPermission : MonoBehaviour {
    //public Text gpsOut;
    public bool isUpdating;
    public GameObject DisableIfDone;
    public GameObject EnableIfDone;

    public UnityEvent EventWhenGotPermission; 

    private void Awake() {
        if (Permission.HasUserAuthorizedPermission(Permission.FineLocation)) {
            if (DisableIfDone != null) {
                DisableIfDone.SetActive(false);
                EnableIfDone.SetActive(true);
            }
        }
    }
    private void Update() {
        /*if (!isUpdating && AskingForGps) {
            StartCoroutine(GetLocation());
        }*/
    }

    public IEnumerator coroutine;
    public void AskForGps() {
        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = GetLocation();
        StartCoroutine(coroutine);
        //AskingForGps = true;
    }

    IEnumerator GetLocation() {
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation)) {
            Permission.RequestUserPermission(Permission.FineLocation);
            //Permission.RequestUserPermission(Permission.CoarseLocation);
        }

#if !UNITY_EDITOR
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
            yield return new WaitForSeconds(10);
#endif

        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes
        int maxWait = 10;
#if !UNITY_EDITOR
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait == 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }
#endif
        // Service didn't initialize in 10 seconds
        if (maxWait < 1)
        {
            //gpsOut.text = "Timed out";
            print("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed) {
            //gpsOut.text = "Unable to determine device location";
            print("Unable to determine device location");
            yield break;
        } else {
            //gpsOut.text = "Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + 100f + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp;
            // Access granted and location value could be retrieved
            print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
            EventWhenGotPermission.Invoke();
        }

        // Stop service if there is no need to query location updates continuously
        isUpdating = !isUpdating;
        Input.location.Stop();
        if (Input.location.status != LocationServiceStatus.Failed) {
            if (DisableIfDone != null) {
                DisableIfDone.SetActive(false);
                EnableIfDone.SetActive(true);
            }
        }
    }
}