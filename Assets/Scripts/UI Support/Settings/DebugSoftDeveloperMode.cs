using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;

public class DebugSoftDeveloperMode : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private float holdTime = 5f;

    private Coroutine holdRoutine;

    public void OnPointerDown(PointerEventData eventData)
    {
        // start counting
        Debug.Log("OnPointerDown");
        holdRoutine = StartCoroutine(HoldTimer());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("OnPointerUp");
        StopHold();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("OnPointerExit");
        StopHold();
    }

    private IEnumerator HoldTimer()
    {
        yield return new WaitForSeconds(holdTime);
        AppSettings.DeveloperMode = true;
        SceneManager.LoadScene("LoadingScene");
    }

    private void StopHold()
    {
        if (holdRoutine != null)
            StopCoroutine(holdRoutine);
        holdRoutine = null;
    }
}