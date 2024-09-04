using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentSleepPreventer : MonoBehaviour
{
    // Singleton instance
    public static PersistentSleepPreventer Instance;

    // This function is called when the script instance is being loaded
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            // Destroy the new instance if there's already an existing one
            Destroy(gameObject);
            return;
        }

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Start is called before the first frame update
    private void Start()
    {
        PreventSleep();
    }

    // Function that runs whenever a scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PreventSleep();
    }

    // Function to prevent the device from going to sleep
    private void PreventSleep()
    {
        // Setting the sleep timeout to 'Never Sleep'
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    // It's a good practice to unsubscribe from events when the object is destroyed
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
