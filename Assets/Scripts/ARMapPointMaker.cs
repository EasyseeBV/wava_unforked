using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Messy.Definitions;
using UnityEngine;

public class ARMapPointMaker : MonoBehaviour {
    
    public GameObject ZoomedInMapObject;
    private OnlineMaps map;
    private OnlineMapsTileSetControl control;

    [Header("Dependencies")]
    [SerializeField] private NoArtworkHandler noArtworkHandler; 
    [SerializeField] private SelectionMenu selectionMenu;
    [SerializeField] private GroupMarkerHandler groupMarker;
    [SerializeField] private LoadingCircle loadingCircle;
    [SerializeField] private GameObject loadingPlane;
    [SerializeField] private NoConnectionMapHandler noConnectionMapHandler;
    
    public static event Action OnHotspotsSpawned;
    public static ArtworkData SelectedArtwork;

    private bool once = false;
    private bool autoCloseOnce = false;
    private bool loadedHotspots = false;
    private bool startedLoading = false;
    
    private void Start() 
    {
        map = OnlineMaps.instance;
        control = OnlineMapsTileSetControl.instance;
        loadingPlane.SetActive(true);
        loadingCircle?.BeginLoading();
        Setup();
    }

    private void OnEnable()
    {
        GroupMarkerHandler.OnGroupsMade += WaitForZoom;
        OnlineMapsTile.OnTileError += OnTileError;
    }

    private void OnTileError(OnlineMapsTile obj)
    {
    }

    private void OnDisable()
    {
        GroupMarkerHandler.OnGroupsMade -= WaitForZoom;
        OnlineMapsTile.OnTileError -= OnTileError;
    }

    //Replaced ARPoint with ARPointSO
    public void InstantiateHotspots()
    {
        
    }

    private async void Setup()
    {
        try
        {
            await FirebaseLoader.LoadRemainingArtworks(() =>
            {
                if (!loadedHotspots || !startedLoading) InstantiateHotspotsAsync();
            });
        }
        catch (Exception e)
        {
            Debug.Log("Failed to load artworks and spawn them in: " + e);
        }
    }

    public async Task InstantiateHotspotsAsync()
    {
        if (loadedHotspots) return;

        try
        {
            startedLoading = true;

            await FirebaseLoader.LoadRemainingExhibitions();
            await Task.Delay(500); // Waits for 1 second

            foreach (var artwork in FirebaseLoader.Artworks)
            {
                try
                {
                    artwork.marker =
                        control.marker3DManager.Create(artwork.longitude, artwork.latitude, ZoomedInMapObject);
                    artwork.marker.instance.name = artwork.title;
                    artwork.hotspot = artwork.marker.instance.GetComponent<HotspotManager>();
                    artwork.marker.sizeType = OnlineMapsMarker3D.SizeType.realWorld;
                    artwork.marker.instance.layer = LayerMask.NameToLayer("Hotspot");
                    artwork.marker.borderTransform = artwork.hotspot.BorderRingMesh.gameObject.transform;
                    artwork.hotspot.Navigation = GetComponent<NavigationMaker>();
                    artwork.hotspot.ConnectedExhibition = await FirebaseLoader.FindRelatedExhibition(artwork.id);
                    artwork.hotspot.Init(artwork);
                    artwork.hotspot.MinZoom = minZoom;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load artwork [{artwork.title}] into map: " + e);
                }
            }

            // Additional event subscriptions and method calls
            map.OnChangeZoom += OnChangeZoom;
            map.OnChangePosition += OnChangePosition;
            OnlineMapsLocationService.instance.OnLocationChanged += OnChangeGps;
            OnChangeGps(OnlineMapsLocationService.instance.position);
            OnChangePosition();
            OnChangeZoom();

            loadingPlane.SetActive(false);
            loadingCircle.StopLoading();
            
            loadedHotspots = true;
            noConnectionMapHandler.Display();
            OnHotspotsSpawned?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to initialize hotspots: " + e);
            loadingPlane.SetActive(false);
            loadingCircle.StopLoading();
            noConnectionMapHandler.ForceDisplay();
        }
    }

    private bool onceZoom = false;
    private void WaitForZoom()
    {
        if (!loadedHotspots || onceZoom) return;
        if (SelectedArtwork != null)
        {
            onceZoom = true;
            HotspotManager.Zoom = 18;
            map.SetPositionAndZoom(SelectedArtwork.longitude, SelectedArtwork.latitude, 18);
            SelectedArtwork.hotspot.ShowSelectionBorder(true);
            //groupMarker.HideAllGroups();
            groupMarker.ZoomGrouping(18);
            StartCoroutine(WaitForEndOfFrameZoomFix());
            SelectedArtwork = null;
        }
    }

    private IEnumerator WaitForEndOfFrameZoomFix()
    {
        yield return new WaitForEndOfFrame();
        groupMarker.ZoomGrouping(18);
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
            if (!anyVisible && !once && FirebaseLoader.Artworks.Count > 0)
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
    private void OnChangeZoom() 
    {
        anyVisible = false;
        
        //Replaced ARPoint with ARPointSO
        foreach (ArtworkData item in FirebaseLoader.Artworks) {
            if (item.hotspot != null)
            {
                item.hotspot.SetFormat(map.zoom);
                item.marker.scale = 1150f * ((Screen.width + Screen.height) / 2f) / Mathf.Pow(2, (map.zoom + map.zoomScale) * 0.85f);
                if (item.marker.inMapView)
                    anyVisible = true;
            }
        }
        UpdateUI();
    }

    private void OnChangePosition() {
        anyVisible = false;

        //Replaced ARPoint with ARPointSO
        foreach (ArtworkData item in FirebaseLoader.Artworks) {
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
        HotspotManager manager = FirebaseLoader.Artworks.OrderBy(t => t.hotspot._distance).FirstOrDefault()?.hotspot;
        if (manager != null && manager.IsClose()) {
            if (ClosestHotspot != null) {
                ClosestHotspot.CanShow = false;
                ClosestHotspot.SetReach(false);
            }

            ClosestHotspot = manager;
            ClosestHotspot.CanShow = true;
            ClosestHotspot.SetReach(true);
        }
        
        foreach (ArtworkData item in FirebaseLoader.Artworks) {
            item.hotspot.OnChangeGpsPosition(OnlineMapsUtils.DistanceBetweenPoints(position, new Vector2((float)item.longitude, (float)item.latitude)).magnitude);
            if (item.hotspot.IsClose()) {
                AnyCloseEnough = true;
            }
        }
        UpdateUI();
    }
}
