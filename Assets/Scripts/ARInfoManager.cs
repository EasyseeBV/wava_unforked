using System.Collections;
using System.Collections.Generic;
using Messy.Definitions;
using UnityEngine;

public class ARInfoManager : MonoBehaviour
{
    public static List<ExhibitionSO> ExhibitionsSO;

    // Start is called before the first frame update
    void Awake()
    {
        if (ExhibitionsSO is not { Count: > 0 })
        {
            ExhibitionsSO = ARStaticInfo.Instance.Exhibitions;
        }
    }
}

[System.Serializable]
public class ARPoint {
    public string Title;
    public string Artist;
    public string Year;
    public string Location;
    public string ShareLink;
    [TextArea(5, 10)]
    public string Description;
    public Sprite ARMapImage;
    public Sprite ARMapBackgroundImage;
    public List<Sprite> ArtworkImages;

    public GameObject ARObject;
    public string AlternateScene;
    public bool PlayARObjectDirectly;
    public bool IsAudio;
    public bool PlaceTextRight;
    public double Latitude, Longitude;
    [HideInInspector]
    public OnlineMapsMarker3D marker = new OnlineMapsMarker3D();
    [HideInInspector]
    public HotspotManager Hotspot;
}