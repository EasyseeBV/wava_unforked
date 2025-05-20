using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NoConnectionMapHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject reconnectContent;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Slider slider;
    
    [Header("Settings")]
    [SerializeField] private bool onHotspotSpawnedCheck = true;
    
    private FirebaseLoader firebaseLoader;
    
    private void Awake()
    {
        content.SetActive(false);
        reconnectContent.SetActive(false);
        
        exitButton.onClick.AddListener(() => content.SetActive(false));
        //retryButton.onClick.AddListener(() => SceneManager.LoadScene(0));
        retryButton.onClick.AddListener(AttemptReconnect);
        
        firebaseLoader = FindObjectOfType<FirebaseLoader>();
    }

    public void TryDisplay() => CheckDisplay();
    public void ForceDisplay() => content.SetActive(true);

    private void CheckDisplay()
    {
        if (FirebaseLoader.Artworks.Count <= 0 && FirebaseLoader.OfflineMode) content.SetActive(true);
    }

    private void AttemptReconnect()
    {
        reconnectContent.SetActive(true);
        content.SetActive(false);

        firebaseLoader.AttemptToReconnect(slider);
        // on reconnect -> reload current scene 
    }
}
