using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets;

public class ARUIPrompts : MonoBehaviour
{
    [Header("Animations")]
    [SerializeField] private Animator scanSurfacePrompt;
    [SerializeField] private Animator tapSurfacePrompt;

    [Header("References")]
    [SerializeField] private TMP_Text scanDescriptionText;
    [SerializeField] private ARInteractorSpawnTrigger spawnTrigger;
    [SerializeField] private ObjectSpawner objectSpawner;

    private bool placed = false;
    
    private void OnEnable()
    {
        spawnTrigger.placementFailed += OnPlacementFailed;
        objectSpawner.objectSpawned += OnObjectPlacement;
    }

    private void OnDisable()
    {
        spawnTrigger.placementFailed -= OnPlacementFailed;
        objectSpawner.objectSpawned -= OnObjectPlacement;
    }

    private void Awake()
    {
        scanSurfacePrompt.gameObject.SetActive(true);
        tapSurfacePrompt.gameObject.SetActive(false);
        StartCoroutine(ShowTapSurfacePrompt());
    }

    private IEnumerator ShowTapSurfacePrompt()
    {
        yield return new WaitForSeconds(2f);
        if (placed) yield return null;
        scanSurfacePrompt.gameObject.SetActive(false);
        tapSurfacePrompt.gameObject.SetActive(true);
    }
    
    private void OnPlacementFailed(int size)
    {
        placed = true;
        tapSurfacePrompt.gameObject.SetActive(false);
        scanDescriptionText.text = $"Scan an area of {size}x{size}m";
        scanSurfacePrompt.gameObject.SetActive(true);
    }
    
    private void OnObjectPlacement(GameObject obj)
    {
        placed = true;
        scanSurfacePrompt.gameObject.SetActive(false);
        tapSurfacePrompt.gameObject.SetActive(false);
    }
}