using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class NavigationMaker : MonoBehaviour
{
    public GameObject MarkerPrefab;
    private OnlineMapsMarker3D marker;

    private OnlineMaps map;
    private OnlineMapsTileSetControl control;
    private OnlineMapsMarker3D targetMarker;

    private OnlineMapsVector2d[] points;
    private OnlineMapsDrawingLine route;
    public static bool IsNavigating;

    private void OnEnable() {
        IsNavigating = false;
        
    }

    // Start is called before the first frame update
    void Start()
    {
        map = OnlineMaps.instance;
        control = OnlineMapsTileSetControl.instance;
        control.marker3DManager.RemoveAll();
        marker = control.marker3DManager.Create(GetPos().x, GetPos().y, MarkerPrefab);
        marker.sizeType = OnlineMapsMarker3D.SizeType.realWorld;
        marker.scale = 1000f / map.zoom;
        //marker.range.min = 0;
        //marker.range.max = 5;

        //control.OnMapClick += SetNavigation;
        OnChangeZoom();
        map.OnChangeZoom += OnChangeZoom;
        OnlineMapsLocationService.instance.OnLocationChanged += OnGpsChanged;
        OnlineMapsLocationService.instance.OnCompassChanged += OnCompassChanged;
        
        FollowPlayer(false);
    }

    public void ZoomIn() {
        FastTravelToPoint(map.position, map.zoom + 1, 0.5f);
    }

    public void ZoomOut() {
        FastTravelToPoint(map.position, map.zoom - 1, 0.5f);
    }

    public void FollowPlayer(bool TurnOn) {
        OnlineMapsLocationService.instance.updatePosition = TurnOn;
    }

    bool IsAnimating;
    float perc;
    Vector2 StartPos = new Vector2();
    Vector2 EndPos = new Vector2(6.773f, 52.23f);
    float startZoom;
    float endZoom = 17f;
    float speed = 1f;

    public void FastTravelToPoint(Vector2 pos, float zoom, float time) {
        IsAnimating = true;
        perc = 0;
        StartPos = map.position;
        startZoom = map.zoom + map.zoomScale;
        EndPos = pos;
        endZoom = zoom;
        speed = 1f / time;
    }

    private void FixedUpdate() {       
        if (IsAnimating) {
            perc += Time.fixedDeltaTime * speed;
            Vector2 curPos = Vector2.Lerp(map.position, EndPos, perc);
            float curZoom = Mathf.Lerp(startZoom, endZoom, perc);
            map.SetPositionAndZoom(curPos.x, curPos.y, curZoom);
            if (perc >= 1.1f)
                IsAnimating = false;
        }

        if (Input.GetKey(KeyCode.W)) {
            GetComponent<OnlineMapsLocationService>().emulatorPosition += new Vector2(0, 0.02f * Time.fixedDeltaTime);
        }
        if (Input.GetKey(KeyCode.S)) {
            GetComponent<OnlineMapsLocationService>().emulatorPosition -= new Vector2(0, 0.02f * Time.fixedDeltaTime);
        }
        if (Input.GetKey(KeyCode.A)) {
            GetComponent<OnlineMapsLocationService>().emulatorPosition -= new Vector2(0.02f * Time.fixedDeltaTime, 0);
        }
        if (Input.GetKey(KeyCode.D)) {
            GetComponent<OnlineMapsLocationService>().emulatorPosition += new Vector2(0.02f * Time.fixedDeltaTime, 0);
        }

        // If the user has left the route, wait for a delay and request a new route
        if (timeToUpdateRoute > 0) {
            timeToUpdateRoute -= Time.fixedDeltaTime;
            if (timeToUpdateRoute <= 0) {
                timeToUpdateRoute = int.MinValue;
                RequestUpdateRoute();
            }
        }
    }
    private void RequestUpdateRoute() {
        SetCustomNavigation(DestinationCoords, DestinationName, null);
    }

        private void OnChangeZoom() {
        marker.scale = 1750f * ((Screen.width + Screen.height) /2f) / Mathf.Pow(2, (map.zoom + map.zoomScale) * 0.85f);
    }

    public Vector2 GetPos() {
        return OnlineMapsLocationService.instance.position;
    }

    public Vector2 DestinationCoords;
    public string DestinationName;

    public void SetNavigation() {
        IsNavigating = true;
        //map.projection.CoordinatesToTile(curPos.x, curPos.y, map.zoom, out tx1, out ty1);
        //map.projection.CoordinatesToTile(targetLng, targetLat, map.zoom, out tx2, out ty2);

        if (!OnlineMapsKeyManager.hasGoogleMaps) {
            Debug.LogWarning("Please enter Map / Key Manager / Google Maps");
            return;
        }
        DestinationCoords = control.GetCoords();
        OnlineMapsGoogleDirections request = new OnlineMapsGoogleDirections(OnlineMapsKeyManager.GoogleMaps(), GetPos(), control.GetCoords());
        request.OnComplete += OnRequestComplete;
        request.Send();
    }

    HotspotManager _manager;
    public void SetCustomNavigation(Vector2 coords, string name, HotspotManager manager) {
        if (_manager != null)
            _manager.Unfocus();
        _manager = manager;
        IsNavigating = true;
        UIInfoController.Instance.SetText(string.Format("Navigation to {0}.", name), 0);
        //map.projection.CoordinatesToTile(curPos.x, curPos.y, map.zoom, out tx1, out ty1);
        //map.projection.CoordinatesToTile(targetLng, targetLat, map.zoom, out tx2, out ty2);

        if (!OnlineMapsKeyManager.hasGoogleMaps) {
            Debug.LogWarning("Please enter Map / Key Manager / Google Maps");
            return;
        }

        DestinationCoords = coords;
        DestinationName = name;
        OnlineMapsGoogleDirections request = new OnlineMapsGoogleDirections(OnlineMapsKeyManager.GoogleMaps(), GetPos(), coords);
        request.OnComplete += OnRequestComplete;
        request.Send();
    }

    private OnlineMapsDrawingLine routeLine;

    private void OnRequestComplete(string response) {
        // Parse a response
        OnlineMapsGoogleDirectionsResult result = OnlineMapsGoogleDirections.GetResult(response);

        // If there are no routes, return
        if (result.routes.Length == 0) {
            Debug.Log("Can't find route");
            return;
        }

        OnlineMapsGoogleDirectionsResult.Route route = result.routes[0];
        if (route == null) {
            Debug.Log("Can't find route");
            return;
        }

        // Reset step and point indices
        currentStepIndex = 0;
        pointIndex = 0;

        // Get steps from the route
        steps = route.legs.SelectMany(l => l.steps).ToArray();

        // Get route points from steps
        routePoints = steps.SelectMany(s => s.polylineD).ToArray();

        // The remaining points are the entire route
        remainPoints = routePoints.ToList();

        // The destination is the last point
        destinationPoint = routePoints.Last();

        // Create a line and add it to the map
        if (routeLine == null) {
            routeLine = new OnlineMapsDrawingLine(remainPoints, Color.black, 2);
            control.drawingElementManager.Add(routeLine);
        } else routeLine.points = remainPoints;

        // Show the whole route
        OnlineMapsGPXObject.Bounds b = route.bounds;

        Vector2[] bounds =
        {
                new Vector2((float) b.minlon, (float) b.maxlat),
                new Vector2((float) b.maxlon, (float) b.minlat),
            };

        Vector2 center;
        int zoom;
        OnlineMapsUtils.GetCenterPointAndZoom(bounds, out center, out zoom);
    }

    /// <summary>
    /// Called when the compass value has been changed.
    /// </summary>
    /// <param name="rotation">Compass true heading (0-1)</param>
    private void OnCompassChanged(float rotation) {
        // Set the rotation of the marker.
        marker.rotationY = rotation * 360f;
    }


    private OnlineMapsVector2d lastPointOnRoute;
    private float timeToUpdateRoute = int.MinValue;
    private OnlineMapsVector2d destinationPoint;

    bool once;
    /// <summary>
    /// Called when the user's GPS position has changed.
    /// </summary>
    /// <param name="position">User's GPS position</param>
    private void OnGpsChanged(Vector2 position) {
        if (!once) {
            OnlineMapsLocationService.instance.UpdatePosition();
            once = true;
        }
        // Set the position of the marker.
        marker.position = position;

        if (IsNavigating) {
            if (GetPointOnRoute(marker.position, out lastPointOnRoute)) {
                // Update covered and remain lines
                UpdateLines();

                // Redraw the map
                map.Redraw();
            }
        }
    }

    const float updateRouteDelay = 10;
    const float updateRouteAfterKm = 0.05f;

    /// <summary>
    /// Finds the nearest point on the route and checks if the user has left the route.
    /// </summary>
    /// <param name="position">User location.</param>
    /// <param name="positionOnRoute">Returns the nearest point on the route.</param>
    /// <param name="pointChanged">Returns whether the number of the route point in use has changed.</param>
    /// <returns>Returns whether the user is following the route.</returns>
    private bool GetPointOnRoute(Vector2 position, out OnlineMapsVector2d positionOnRoute) {
        var step = steps[currentStepIndex];
        OnlineMapsVector2d p1 = step.polylineD[pointIndex];
        OnlineMapsVector2d p2 = step.polylineD[pointIndex + 1];
        OnlineMapsVector2d p;
        double dist;

        if (p1 != p2) {
            // Check if the user is on the same route point.
            OnlineMapsUtils.NearestPointStrict(position.x, position.y, p1.x, p1.y, p2.x, p2.y, out p.x, out p.y);
            if (p != p2) {
                dist = OnlineMapsUtils.DistanceBetweenPoints(p.x, p.y, 0, position.x, position.y, 0);

                if (dist < updateRouteAfterKm) {
                    timeToUpdateRoute = int.MinValue;
                    positionOnRoute = p;
                    return true;
                }
            }
        }

        // Checking what step and point the user is on
        for (int i = currentStepIndex; i < steps.Length; i++) {
            step = steps[i];
            OnlineMapsVector2d[] polyline = step.polylineD;

            for (int j = pointIndex; j < polyline.Length - 1; j++) {
                p1 = polyline[j];
                p2 = polyline[j + 1];
                OnlineMapsUtils.NearestPointStrict(position.x, position.y, p1.x, p1.y, p2.x, p2.y, out p.x, out p.y);
                if (p == p2) continue;

                dist = OnlineMapsUtils.DistanceBetweenPoints(p.x, p.y, 0, position.x, position.y, 0);
                if (dist < updateRouteAfterKm) {
                    currentStepIndex = i;
                    pointIndex = j;
                    timeToUpdateRoute = int.MinValue;
                    positionOnRoute = p;
                    return true;
                }
            }

            pointIndex = 0;
        }

        // The user has left the route. If the countdown to the search for a new route has not started, we start it.
        if (timeToUpdateRoute < -999) timeToUpdateRoute = updateRouteDelay;

        positionOnRoute = lastPointOnRoute;
        return false;
    }

    private List<OnlineMapsVector2d> remainPoints;
    private List<OnlineMapsVector2d> coveredPoints;
    private OnlineMapsGoogleDirectionsResult.Step[] steps;
    private OnlineMapsVector2d[] routePoints;
    private int currentStepIndex = -1;
    private int pointIndex = -1;

    /// <summary>
    /// Updates covered and remain lines
    /// </summary>
    private void UpdateLines() {
        // Clears line points.
        // It doesn't make sense to create new lines here, because drawing elements keeps a reference to the lists.
        coveredPoints.Clear();
        remainPoints.Clear();

        // Iterate all steps.
        for (int i = 0; i < steps.Length; i++) {
            // Get a polyline
            var step = steps[i];
            OnlineMapsVector2d[] polyline = step.polylineD;

            // Iterate all points of polyline
            for (int j = 0; j < polyline.Length; j++) {
                OnlineMapsVector2d p = polyline[j];

                // If index of step less than current step, add to covered list
                // If index of step greater than current step, add to remain list
                // If this is current step, points than less current point add to covered list, otherwise add to remain list
                if (i < currentStepIndex) {
                    coveredPoints.Add(p);
                } else if (i > currentStepIndex) {
                    remainPoints.Add(p);
                } else {
                    if (j < pointIndex) {
                        coveredPoints.Add(p);
                    } else if (j > pointIndex) {
                        remainPoints.Add(p);
                    } else {
                        coveredPoints.Add(p);
                        coveredPoints.Add(lastPointOnRoute);
                        remainPoints.Add(lastPointOnRoute);
                    }
                }
            }
        }
    }
}
