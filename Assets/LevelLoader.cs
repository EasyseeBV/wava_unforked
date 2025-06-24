using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image wavaImage;
    [SerializeField] private RectTransform iconImage;
    [SerializeField] private GameObject offlineWarningContainer;
    [SerializeField] private TextFader wavaDescriptionTextFader;

    [Header("Settings")]
    [SerializeField] [Range(0, 3)] private float wavaFadeSpeed = 0.6f;
    [SerializeField] [Range(0, 3)] private float iconMoveSpeed = 0.5f;
    [SerializeField] [Range(0, 3)] private float transitionWaitTime = 0.5f;

    private bool transitionComplete = false;

    public static string DebugSceneToOpen = string.Empty;

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();


        // Show / hide no internet warning
        offlineWarningContainer.SetActive(Application.internetReachability == NetworkReachability.NotReachable);


        transitionComplete = false;

        int levelToLoad = PlayerPrefs.GetInt("OpeningTutorial", 0);
        string level = levelToLoad == 0 ? "WelcomeTutorial_1" : "Map";
        
        if (!string.IsNullOrEmpty(DebugSceneToOpen)) level =  DebugSceneToOpen;
        
        StartCoroutine(LoadAsynchronously(level));
    }

    IEnumerator LoadAsynchronously(string sceneStr)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneStr);
        operation.allowSceneActivation = false;
        
        while (!operation.isDone)
        {
            // Allow scene transitions
            if (operation.progress >= 0.9f && transitionComplete && FirebaseLoader.Initialized && FirebaseLoader.SetupComplete)
            {
                operation.allowSceneActivation = true;
            }
            else if(operation.progress >= 0.9f && !transitionComplete && FirebaseLoader.Initialized && FirebaseLoader.SetupComplete)
            {
                //loadingImage.gameObject.SetActive(false);
                iconImage.gameObject.SetActive(true);
                
                float elapsedTime = 0f;
                Color startColor = wavaImage.color;
                Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

                wavaDescriptionTextFader.FadeOut();

                while (elapsedTime < wavaFadeSpeed)
                {
                    elapsedTime += Time.deltaTime;
                    wavaImage.color = Color.Lerp(startColor, endColor, elapsedTime / wavaFadeSpeed);
                    yield return null;
                }

                if (elapsedTime >= wavaFadeSpeed)
                {
                    Vector2 startPosition = iconImage.anchoredPosition;
                    Vector2 endPosition = new Vector2(0, iconImage.anchoredPosition.y);
                    float iconElapsedTime = 0f;

                    while (iconElapsedTime < iconMoveSpeed)
                    {
                        iconElapsedTime += Time.deltaTime;
                        float t = iconElapsedTime / iconMoveSpeed;
                        iconImage.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);
                        yield return null;
                    }
                    
                    yield return new WaitForSeconds(transitionWaitTime);
                    transitionComplete = true;
                }
            }
            
            yield return null;  
        }
    }
}
