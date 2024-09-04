using UnityEngine;

public class ModelTopFinder : MonoBehaviour
{
    public GameObject prefabInstance;  // Drag your prefab instance here in the Inspector

    public GameObject PrefabToPlace;

    public void FindTopmostPointOfModels()
    {
        if (prefabInstance == null)
        {
            Debug.LogError("Prefab instance is not set.");
            return;
        }

        float highestPoint = float.MinValue;
        GameObject highestObject = null;

        MeshFilter[] meshFilters = prefabInstance.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.sharedMesh != null)
            {
                float localTop = mf.sharedMesh.bounds.center.y + mf.sharedMesh.bounds.extents.y;
                float worldTop = mf.transform.TransformPoint(new Vector3(0, localTop, 0)).y;

                if (worldTop > highestPoint)
                {
                    highestPoint = worldTop;
                    highestObject = mf.gameObject;
                }
            }
        }
        if (highestObject == null)
            highestPoint = 0;

        GameObject TopUI = Instantiate(PrefabToPlace, prefabInstance.transform);
        TopUI.transform.parent = prefabInstance.transform;
        bool FoundAnObject = highestObject != null;
        TopUI.transform.position = new Vector3(0, highestPoint + (FoundAnObject ? 0.5f : 1.5f), 0);
        TopUI.GetComponent<TopInfoViewer>().ShowInfoOnTop(FoundAnObject);

        if (highestObject != null)
        {
            Debug.Log($"Object '{highestObject.name}' has the topmost point at Y: {highestPoint}");
        }
        else
        {
            Debug.Log("No suitable models found in the prefab.");
        }
    }
}
