using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Image loadingImage;
    [SerializeField] private Image wavaImage;
    [SerializeField] private RectTransform iconImage;
    
    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] [Range(0, 3)] private float wavaFadeSpeed = 0.6f;
    [SerializeField] [Range(0, 3)] private float iconMoveSpeed = 0.5f;
    [SerializeField] [Range(0, 3)] private float transitionWaitTime = 0.5f;
    
    private bool transitionComplete = false;

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        loadingScreen.SetActive(true);
        transitionComplete = false;

        int levelToLoad = PlayerPrefs.GetInt("OpeningTutorial", 0);
        string level = levelToLoad == 0 ? "WelcomeTutorial_1" : "Home";
        
        StartCoroutine(LoadAsynchronously(level));
    }

    IEnumerator LoadAsynchronously(string sceneStr)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneStr);
        operation.allowSceneActivation = false;
        
        while (!operation.isDone)
        {
            if (operation.progress < 0.9f)
            {
                loadingImage.transform.Rotate(Vector3.forward * (rotationSpeed * Time.deltaTime));
            }
            else if (operation.progress >= 0.9f && transitionComplete)
            {
                operation.allowSceneActivation = true;
            }
            else if(operation.progress >= 0.9f && !transitionComplete)
            {
                loadingImage.gameObject.SetActive(false);
                iconImage.gameObject.SetActive(true);
                
                float elapsedTime = 0f;
                Color startColor = wavaImage.color;
                Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

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
