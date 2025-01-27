using Messy.Definitions;
using UnityEngine;

public class MoveMapToArtwork : MonoBehaviour
{
    [SerializeField] private OnlineMaps maps;

    public void Move(ArtworkData artwork)
    {
        maps.SetPosition(artwork.longitude, artwork.latitude);
    }
}
