using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFinder : MonoBehaviour
{
    [SerializeField] private OnlineMaps maps;
    [SerializeField] private Button findButton;

    private void Awake()
    {
        findButton.onClick.AddListener(GoToPlayer);
    }

    private void GoToPlayer()
    {
        var mapMarker = PlayerMarker.Instance.MapsMarker;
        maps.SetPosition(mapMarker.Longitude, mapMarker.Latitude);
    }
}
