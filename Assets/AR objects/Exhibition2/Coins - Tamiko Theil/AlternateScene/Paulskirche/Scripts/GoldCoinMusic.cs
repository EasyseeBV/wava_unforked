using UnityEngine;

public class GoldCoinMusic : MonoBehaviour
{
    public GameObject Music;

    AudioSource coinFall = null;
    private void OnCollisionEnter(Collision collision)
    {
        if (coinFall != null)
        {
            coinFall.Play();
            coinFall = null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        coinFall = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                if (hit.transform == gameObject.transform)
                {
                    GameObject newMusic = Instantiate(Music, gameObject.transform.position, gameObject.transform.rotation);
                    newMusic.SetActive(true);
                }
            }
        }
    }

}
