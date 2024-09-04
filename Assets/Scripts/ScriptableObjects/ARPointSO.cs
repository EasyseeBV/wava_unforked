using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace Messy.Definitions
{
    [CreateAssetMenu(fileName = "ARPointSO.asset", menuName = "Wava Artwork/AR Point",
        order = 120)]
    [System.Serializable]
    public class ARPointSO : ScriptableObject
    {
        public string Title;
        public string Artist;
        public List<ArtistSO> Artists;
        public string Year;
        public string Location;
        public string ShareLink;
        public string QR;
        [TextArea(5, 10)] public string Description;
        public Sprite ARMapImage; 
        public Sprite ARMapBackgroundImage;
        public List<Sprite> ArtworkImages;

        public AssetReferenceGameObject ARObjectReference;
        public string AlternateScene;
        public bool PlayARObjectDirectly;
        public bool IsAudio;
        public bool PlaceTextRight;
        public double Latitude, Longitude;
        public float MaxDistance;
        [HideInInspector] public OnlineMapsMarker3D marker = new OnlineMapsMarker3D();
        [HideInInspector] public HotspotManager Hotspot;
        [HideInInspector] public bool Liked;
    }
}
