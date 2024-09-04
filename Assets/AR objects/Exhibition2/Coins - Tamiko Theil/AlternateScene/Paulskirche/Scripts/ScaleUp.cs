using UnityEngine;

public class ScaleUp : MonoBehaviour
{
    public GameObject Doc;
    public GameObject Boom;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HitTarget(Input.mousePosition);
        }
        if (Input.touchCount > 0) { 
            HitTarget(Input.GetTouch(0).position);
        }
    }

    public void HitTarget(Vector3 TouchPosition) {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(TouchPosition);

        if (Physics.Raycast(ray, out hit, 100.0f)) {
            if (hit.transform == gameObject.transform) {
                GameObject newDoc = Instantiate(Doc, gameObject.transform.position, gameObject.transform.rotation);
                newDoc.SetActive(true);
                gameObject.SetActive(false);

                GameObject newBoom = Instantiate(Boom);
                newBoom.SetActive(true);
            }
        }
    }
}
