using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBootstrapper : MonoBehaviour
{
    [SerializeField] private StartScreenManager startScreenManager;

    private void Awake()
    {
        StartScreenManager.Once = true;
    }
}
