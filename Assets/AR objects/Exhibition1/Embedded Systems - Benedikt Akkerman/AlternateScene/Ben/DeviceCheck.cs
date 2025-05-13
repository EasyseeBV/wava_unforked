using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeviceCheck : MonoBehaviour
{
    [System.Serializable]
    private enum Device
    {
        Android,
        iOS
    }
    
    [SerializeField] private Device dontLoadOnDevice;
    [SerializeField] private GameObject showOnFail;
    [SerializeField] private string sceneToLoad;
    
    private void Awake()
    {
        // Determine current runtime platform
        bool isDontLoadDevice = false;

#if UNITY_ANDROID
        isDontLoadDevice = (dontLoadOnDevice == Device.Android);
#elif UNITY_IOS
        isDontLoadDevice = (dontLoadOnDevice == Device.iOS);
#else 
        // Other platforms are treated as supported by default
        isDontLoadDevice = false;
#endif

        if (isDontLoadDevice)
        {
            if (showOnFail != null) showOnFail.SetActive(true);
        }
        else
        {
            // Proceed to load the specified scene
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                SceneManager.LoadScene("Map");
            }
        }
    }
}
