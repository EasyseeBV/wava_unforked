using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShowObjectsScripts : MonoBehaviour
{
	//public GOMap goMap;
	public List<ObjectToSpawn> ObjectsToSpawn;
	public Button button;
	public GameObject Avatar;

	[System.Serializable]
	public class ObjectToSpawn{
		public string Name;
		public GameObject ObjectToShowOnMap;
		public GameObject ObjectToShowInAR;
		public bool SpawnDirectly;
		//public Coordinates co;
	}

	

	// Use this for initialization
	IEnumerator Start()
	{
		//Wait for the location manager to have the world origin set.
		//yield return StartCoroutine(goMap.locationManager.WaitForOriginSet());
		yield return null;
        //[NOTE THAT THIS EXAMPLE ONLY WORKS IN "PARIS" demo scene] 
        foreach (var item in ObjectsToSpawn)
        {
			print(item.ObjectToShowInAR);
			//Drop a point on the map
			
			//Temporarily suppressing this behavior from executing, to ensure that only ARTapper does the spawning
			//This script needs to be updated to use asset references
			//DropPin(item);
		}
	}

	public void Reveal(ObjectToSpawn objectData)
    {	
	    //Temporarily suppressing this behavior from executing, to ensure that only ARTapper does the spawning
	    //This script needs to be updated to use asset references
		//ArTapper.ARPointToPlace.ARObject = objectData.ObjectToShowInAR;
		
		ArTapper.PlaceDirectly = objectData.SpawnDirectly;
		ArTapper.DistanceWhenActivated = Vector3.Distance(objectData.ObjectToShowOnMap.transform.position, Avatar.transform.position);
		SceneManager.LoadScene("AR");
    }

	private void Update()
	{
		/*if(objectData != null)
        {
			print(Vector3.Distance(objectData.transform.position, Avatar.transform.position));
			if (Vector3.Distance(objectData.transform.position, Avatar.transform.position) < 100)
            {
				button.interactable = true;
				button.GetComponentInChildren<TextMeshProUGUI>().text = "Tap to reveal in AR!";
            }
            else
            {
				button.interactable = false;
				button.GetComponentInChildren<TextMeshProUGUI>().text = "Get closer to the object.";
			}
		}*/
		foreach (var item in ObjectsToSpawn)
		{
			if (Vector3.Distance(item.ObjectToShowOnMap.transform.position, Avatar.transform.position) < 50)
			{
				item.ObjectToShowOnMap.GetComponent<ObjectData>().button.gameObject.SetActive(true);
            }
            else {
				item.ObjectToShowOnMap.GetComponent<ObjectData>().button.gameObject.SetActive(false);
			}
		}

		/*if (Input.GetMouseButtonDown(0))
        {
			Ray raycast = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit raycastHit;
			if (Physics.Raycast(raycast, out raycastHit))
			{
				if (raycastHit.transform.tag == "ARObject")
				{	
					objectData = raycastHit.transform.GetComponentInParent<ObjectData>();
				}

			}
		}

		if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
		{
			Ray raycast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
			RaycastHit raycastHit;
			if (Physics.Raycast(raycast, out raycastHit))
			{
				if (raycastHit.transform.tag == "ARObject")
				{
					objectData = raycastHit.transform.GetComponentInParent<ObjectData>();
				}

			}
		}*/
	}

    void DropPin(ObjectToSpawn ots)
	{
		//1) create game object (you can instantiate a prefab instead)
		ots.ObjectToShowOnMap = Instantiate(ots.ObjectToShowOnMap);
		ObjectData objectSpawner = ots.ObjectToShowOnMap.GetComponent<ObjectData>();
		print(objectSpawner);
		objectSpawner.ObjectToSpawn = ots;
		objectSpawner.text.text = ots.Name;
		objectSpawner.button.onClick.AddListener(delegate { Reveal(ots); });

		//2) make a Coordinate class with your desired latitude longitude
		//Coordinates coordinates = new Coordinates(ots.co.latitude, ots.co.longitude);

		//3) call drop pin passing the coordinates and your gameobject
		//goMap.dropPin(coordinates, ots.ObjectToShowOnMap);
		Vector3 pos = ots.ObjectToShowOnMap.transform.position;
		pos.y = 0;
		ots.ObjectToShowOnMap.transform.position = pos;
	}
}