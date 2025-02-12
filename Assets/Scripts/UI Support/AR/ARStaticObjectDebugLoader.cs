using System;
using System.Collections;
using System.Collections.Generic;
using Messy.Definitions;
using UnityEngine;

public class ARStaticObjectDebugLoader : MonoBehaviour
{
    public ArtworkData Artwork;
    public bool AddARPoint = false;

    private void Start()
    {
        AddARPoint = true;
    }

    void Update()
    {
        if (AddARPoint)
        {
            AddARPoint = false;
            ArTapper.ArtworkToPlace = Artwork;
        }
    }
}
