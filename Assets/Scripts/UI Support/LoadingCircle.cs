using System.Collections;
using UnityEngine;

public class LoadingCircle : MonoBehaviour
{
    public bool Loading { get; set; } = false;
    
    private const float rotationSpeed = 100f;

    public void BeginLoading()
    {
        if (!gameObject.activeInHierarchy) return;
        
        Loading = true;
        StartCoroutine(Rotate());
    }

    public void StopLoading()
    {
        Loading = false;
        gameObject.SetActive(false);
    }

    private IEnumerator Rotate()
    {
        while (Loading)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
