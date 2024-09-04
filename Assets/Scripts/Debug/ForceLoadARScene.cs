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
    [SerializeField] private ARPointSO arPointSo;

    private void Awake()
    {
        btn.onClick.AddListener(() => StartAR(arPointSo));
    }

    public void StartAR(ARPointSO point)
    {
        ArTapper.ARPointToPlace = point;
        ArTapper.PlaceDirectly = point.PlayARObjectDirectly;
        ArTapper.DistanceWhenActivated = 100f;

        SceneManager.LoadSceneAsync("AR");
    }
}
