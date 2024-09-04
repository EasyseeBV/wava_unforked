using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;


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
    }
}
