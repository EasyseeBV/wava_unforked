using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARAudioObject : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    private AudioClip cachedAudioClip;
    
    public void PlayAudio()
    {
        audioSource.clip = cachedAudioClip;
        audioSource.Play();
    }

    public void StoreAudio(AudioClip clip)
    {
        cachedAudioClip = clip;
    }
}
