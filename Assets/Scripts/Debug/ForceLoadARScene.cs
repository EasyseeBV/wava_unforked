using System;
using System.Collections;
using System.Collections.Generic;
using Messy.Definitions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ForceLoadARScene : MonoBehaviour
{
    [SerializeField] private Button btn;
    [SerializeField] private ArtworkData arPointSo;

    private void Awake()
    {
        btn.onClick.AddListener(() => StartAR(arPointSo));
    }

    public void StartAR(ArtworkData point)
    {
        ArTapper.ArtworkToPlace = point;
        ArTapper.PlaceDirectly = point.place_right;
        ArTapper.DistanceWhenActivated = 100f;

        SceneManager.LoadSceneAsync("AR");
    }
}
