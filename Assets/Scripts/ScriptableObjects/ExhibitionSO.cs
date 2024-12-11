using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Messy.Definitions
{

    [CreateAssetMenu(fileName = "ExhibitionSO.asset", menuName = "Wava Artwork/Exhibition",
order = 121)]
    [System.Serializable]
    public class ExhibitionSO : ScriptableObject {
        public string Title;
        public string Artist;
        public string Year;
        public string Location;
        public Color32 Color;
        [TextArea(5, 10)] public string Description;
        public List<Sprite> ExhibitionImages;
        public List<ARPointSO> ArtWorks = new List<ARPointSO>();
        public Sprite ARMapIcon;
        [HideInInspector] public bool Liked;

        [Space, ReadOnly] public long creationDateTime;

        private void OnValidate()
        {
            if (creationDateTime == 0)
            {
                creationDateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                Debug.Log($"Updated the creation date time of the exhibition: {name} | {DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
            }
        }
    }
}
