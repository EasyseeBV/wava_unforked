using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFinder : MonoBehaviour
{
    [SerializeField] private OnlineMaps maps;
    [SerializeField] private Button findButton;
    [SerializeField] private GroupMarkers groupMarkers;

    private void Awake()
    {
        findButton.onClick.AddListener(GoToPlayer);
    }

    private void GoToPlayer()
    {
        var mapMarker = PlayerCollider.Instance.MapsMarker;
        maps.SetPosition(mapMarker.Longitude, mapMarker.Latitude);
    }
}
