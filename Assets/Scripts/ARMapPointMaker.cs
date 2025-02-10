using System;
using System.Collections;
using System.Linq;
using Messy.Definitions;
using UnityEngine;

public class ARMapPointMaker : MonoBehaviour {
    
    public GameObject ZoomedInMapObject;
    private OnlineMaps map;
    private OnlineMapsTileSetControl control;

    [Header("Dependencies")]
    [SerializeField] private GroupMarkers groupMarkers;
    [SerializeField] private NoArtworkHandler noArtworkHandler;

    public static event Action OnHotspotsSpawned;

    private bool once = false;
    private bool autoCloseOnce = false;
    
    void Start() {
        map = OnlineMaps.instance;
        control = OnlineMapsTileSetControl.instance;
    }
    
    //Replaced ARPoint with ARPointSO
    public void InstantiateHotspots() {

        StartCoroutine(IEInstatiateHotspot());
    }

    public IEnumerator IEInstatiateHotspot()
    {
        yield return new WaitForSecondsRealtime(3f);

        foreach (var artwork in FirebaseLoader.Artworks)
        {
            Debug.Log("Loading an artwork point...");
            artwork.marker = control.marker3DManager.Create(artwork.longitude, artwork.latitude, ZoomedInMapObject);
            artwork.marker.instance.name = artwork.title;
            artwork.hotspot = artwork.marker.instance.GetComponent<HotspotManager>();
            artwork.marker.sizeType = OnlineMapsMarker3D.SizeType.realWorld;
            artwork.marker.instance.layer = LayerMask.NameToLayer("Hotspot");
            artwork.marker.borderTransform = artwork.hotspot.BorderRingMesh.gameObject.transform;
            artwork.hotspot.Navigation = GetComponent<NavigationMaker>();
            artwork.hotspot.ConnectedExhibition = FirebaseLoader.Exhibitions[0];
            Debug.LogWarning("ConnectedExhibition removed temp");
            artwork.hotspot.Init(artwork);
            artwork.hotspot.MinZoom = minZoom;
            artwork.hotspot.OnChangeGpsPosition(OnlineMapsUtils.DistanceBetweenPoints(OnlineMapsLocationService.instance.position, new Vector2((float)artwork.longitude, (float)artwork.latitude)).magnitude);

        }
        
        foreach (ExhibitionData exhibition in FirebaseLoader.Exhibitions)
        {
            foreach (ArtworkData artwork in exhibition.artworks)
            {
                artwork.marker = control.marker3DManager.Create(artwork.longitude, artwork.latitude, ZoomedInMapObject);
                artwork.marker.instance.name = artwork.title;
                artwork.hotspot = artwork.marker.instance.GetComponent<HotspotManager>();
                artwork.marker.sizeType = OnlineMapsMarker3D.SizeType.realWorld;
                artwork.marker.instance.layer = LayerMask.NameToLayer("Hotspot");
                artwork.marker.borderTransform = artwork.hotspot.BorderRingMesh.gameObject.transform;
                artwork.hotspot.Navigation = GetComponent<NavigationMaker>();
                artwork.hotspot.ConnectedExhibition = exhibition;
                artwork.hotspot.Init(artwork);
                artwork.hotspot.MinZoom = minZoom;
                artwork.hotspot.OnChangeGpsPosition(OnlineMapsUtils.DistanceBetweenPoints(OnlineMapsLocationService.instance.position, new Vector2((float)artwork.longitude, (float)artwork.latitude)).magnitude);
                
                /*if (SelectionMenu.SelectedARPoint == artwork)
                {
                    SelectionMenu.Instance.LoadARPointSO();
                }*/
            }
        }

        map.OnChangeZoom += OnChangeZoom;
        map.OnChangePosition += OnChangePosition;
        OnlineMapsLocationService.instance.OnLocationChanged += OnChangeGps;
        OnChangeGps(OnlineMapsLocationService.instance.position);
        OnChangePosition();
        OnChangeZoom();
        OnHotspotsSpawned?.Invoke();
        //groupMarkers?.Group();
    }

    bool StartedTouch;
    RaycastHit hit;
    private void Update() 
    {
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began) {
            if (IsTouchOverThisObject(Input.GetTouch(0))) {
                StartedTouch = true;
            }
        } else if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended) {
            if (StartedTouch && hit.transform.TryGetComponent(out HotspotManager manager)) {
                manager.OnTouch();
            }
            StartedTouch = false;
        } else if (Input.touchCount > 1 || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)) {
            StartedTouch = false;
        }


        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 4000.0f, ~LayerMask.NameToLayer("Hotspot"))) {
                if (hit.transform.TryGetComponent(out HotspotManager manager)) {
                    manager.OnTouch();
                }
            }
        }
    }
    
    void UpdateUI()
    {
        if (!NavigationMaker.IsNavigating)
        {
            if (!anyVisible && !once)
            {
                once = true;
                autoCloseOnce = true;
                noArtworkHandler.Open();
            }
            else if(anyVisible && autoCloseOnce)
            {
                autoCloseOnce = false;
                noArtworkHandler.Close();
            }
        }

        return;
        
        if (!NavigationMaker.IsNavigating) {
            if (AnyCloseEnough) {
                UIInfoController.Instance.SetText("Tap “Enter WAVA” to view Artwork.", 0);
            } else if (!anyVisible) {
                UIInfoController.Instance.SetText("No artwork (        ) near you. Zoom out.", 1);
            } else if (map.zoom < minZoom) {
                UIInfoController.Instance.SetText("Tap          to zoom in", 2);
            } else if (!AnyCloseEnough) {
                UIInfoController.Instance.SetText("Tap          to navigate", 2);
            }
        }
    }

    bool IsTouchOverThisObject(Touch touch) {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(touch.position.x, touch.position.y, 0));  
        return Physics.Raycast(ray, out hit, 4000.0f, ~LayerMask.NameToLayer("Hotspot"));
    }
    const int minZoom = 12;
    bool anyVisible = false;
    private void OnChangeZoom() {
        anyVisible = false;
        
        //Replaced ARPoint with ARPointSO
        foreach (ArtworkData item in FirebaseLoader.Exhibitions.SelectMany(s => s.artworks)) {
            item.hotspot.SetFormat(map.zoom);
            item.marker.scale = 1150f * ((Screen.width + Screen.height) / 2f) / Mathf.Pow(2, (map.zoom + map.zoomScale) * 0.85f);
            if (item.marker.inMapView)
                anyVisible = true;
        }
        UpdateUI();
    }

    private void OnChangePosition() {
        anyVisible = false;

        //Replaced ARPoint with ARPointSO
        foreach (ArtworkData item in FirebaseLoader.Exhibitions.SelectMany(s => s.artworks)) {
            if (item.marker.inMapView) {
                anyVisible = true;
            }
        }
        UpdateUI();
    }

    bool AnyCloseEnough = false;

    HotspotManager ClosestHotspot;
    /// <summary>
    /// Called when the user's GPS position has changed.
    /// </summary>
    /// <param name="position">User's GPS position</param>
    private void OnChangeGps(Vector2 position) {
        AnyCloseEnough = false;

        // adjust to work better - it breaks right now
        HotspotManager manager = FirebaseLoader.Exhibitions.SelectMany(s => s.artworks).OrderBy(t => t.hotspot._distance).FirstOrDefault().hotspot;
        if (manager.IsClose()) {
            if (ClosestHotspot != null) {
                ClosestHotspot.CanShow = false;
                ClosestHotspot.SetReach(false);
            }

            ClosestHotspot = manager;
            ClosestHotspot.CanShow = true;
            ClosestHotspot.SetReach(true);
        }

        //Replaced ARPoint with ARPointSO
        foreach (ArtworkData item in FirebaseLoader.Exhibitions.SelectMany(s => s.artworks)) {
            item.hotspot.OnChangeGpsPosition(OnlineMapsUtils.DistanceBetweenPoints(position, new Vector2((float)item.longitude, (float)item.latitude)).magnitude);
            if (item.hotspot.IsClose()) {
                AnyCloseEnough = true;
            }
        }
        UpdateUI();
    }
}
