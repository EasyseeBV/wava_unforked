using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SimpleSceneLoader : MonoBehaviour
{
    public Image LoadingImage;

    public void LoadScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneAsync(string sceneName) {
        StartCoroutine(LoadAsynchronously(sceneName));
    }

    IEnumerator LoadAsynchronously(string sceneName) {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone) {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            if (LoadingImage != null)
                LoadingImage.fillAmount = progress;
            
            yield return null;
        }
    }

    public void SetToArtWork() {
        ArtworkUIManager.SelectedArtworks = true;
    }

    public void SetToExhibition() {
        ArtworkUIManager.SelectedArtworks = false;
    }
}
