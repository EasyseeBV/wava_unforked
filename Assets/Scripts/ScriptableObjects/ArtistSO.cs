using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Messy.Definitions
{
    [CreateAssetMenu(fileName = "ArtistSO.asset", menuName = "Wava Artwork/Artist",
order = 122)]
    [System.Serializable]
    public class ArtistSO : ScriptableObject
    {
        public string Title;
        public string Location;
        public string Link;
        public Sprite ArtistIcon;

        [TextArea(5, 10)] public string Description;
        [HideInInspector] public bool Liked;
    }
}