using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeInOut : MonoBehaviour
{
    #region FIELDS
    public Image fadeOutUIImage;
    public float fadeSpeed = 0.8f;
    public bool SwtichScene;
    public string SceneName;
    public bool fadeOnStart;


    public enum FadeDirection
    {
        In, //Alpha = 1
        Out // Alpha = 0
    }
    #endregion

    #region MONOBHEAVIOR
    void OnEnable()
    {
        //fadeOutUIImage = fadeOutUIImage.GetComponent<Image>();
        //if (SwtichScene)
        //{
        //    StartCoroutine(_FadeAndLoadScene(FadeDirection.Out, SceneName));
        //}
        //StartCoroutine(_Fade(FadeDirection.Out));
        if (fadeOnStart)
        {
           mFade(FadeDirection.Out);
        }
    }
    #endregion

    public void LoadScenee(string sceneName)
    {
        mFadeAndLoadScene(FadeDirection.In, sceneName);
    }

    public void mFade(FadeDirection fadeDirection)
    {
        StartCoroutine(Fade(fadeDirection));
    }

    public void mFadeAndLoadScene(FadeDirection fadeDirection, string SceneName)
    {
        StartCoroutine(FadeAndLoadScene(fadeDirection, SceneName));
    }

    #region FADE
    private IEnumerator Fade(FadeDirection fadeDirection)
    {
        float alpha = (fadeDirection == FadeDirection.Out) ? 1 : 0;
        float fadeEndValue = (fadeDirection == FadeDirection.Out) ? 0 : 1;
        if (fadeDirection == FadeDirection.Out)
        {
            while (alpha >= fadeEndValue)
            {
                SetColorImage(ref alpha, fadeDirection);
                yield return null;
            }
            fadeOutUIImage.enabled = false;
        }
        else
        {
            fadeOutUIImage.enabled = true;
            while (alpha <= fadeEndValue)
            {
                SetColorImage(ref alpha, fadeDirection);
                yield return null;
            }
        }
    }
    #endregion

    #region HELPERS
    public IEnumerator FadeAndLoadScene(FadeDirection fadeDirection, string sceneToLoad)
    {
        yield return Fade(fadeDirection);
        SceneManager.LoadScene(sceneToLoad);
    }

    private void SetColorImage(ref float alpha, FadeDirection fadeDirection)
    {
        fadeOutUIImage.color = new Color(fadeOutUIImage.color.r, fadeOutUIImage.color.g, fadeOutUIImage.color.b, alpha);
        alpha += Time.deltaTime * (1.0f / fadeSpeed) * ((fadeDirection == FadeDirection.Out) ? -1 : 1);
    }
    #endregion

}
