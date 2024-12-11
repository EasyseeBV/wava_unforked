using System;
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
        
        [Space, ReadOnly] public long creationDateTime;
        
        private void OnValidate()
        {
            if (creationDateTime == 0)
            {
                creationDateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                Debug.Log($"Updated the creation date time of the artist: {name} | {DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
            }
        }
    }
}