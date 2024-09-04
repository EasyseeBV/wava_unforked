using Messy.Definitions;
using UnityEngine;

public class MoveMapToArtwork : MonoBehaviour
{
    [SerializeField] private OnlineMaps maps;

    public void Move(ARPointSO arPointSo)
    {
        maps.SetPosition(arPointSo.Longitude, arPointSo.Latitude);
    }
}
