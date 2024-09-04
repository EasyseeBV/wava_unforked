using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static OnlineMapsWhat3Words;

public class AudioController : MonoBehaviour
{
    //Configuration Settings in Scene
    [SerializeField] public AudioSource finalAudio;
    [SerializeField] public Transform audioSourcesParent;

    public Dictionary<string, bool> playedClips = new Dictionary<string, bool>();
    public Dictionary<string, AudioSource> audioClips = new Dictionary<string, AudioSource>();

    public float timeLeft;
    public bool timerOn = false;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI clipsRemainingText;
    public TextMeshProUGUI clipsScannedText;

    private string origTimerText;
    private string origClipsRemainingText;
    private string origClipsScannedText;

    public int clipsScanned;
    public int clipsRemaining;



    public void Awake()
    {
        origClipsRemainingText = clipsRemainingText.text;
        origClipsScannedText = clipsScannedText.text;
        origTimerText = timerText.text;

        ResetScene();

        var audioSources = audioSourcesParent.GetComponentsInChildren<AudioSource>().ToList();

        foreach (var clip in audioSources)
        {
            audioClips.Add(clip.gameObject.name, clip);
            playedClips.Add(clip.gameObject.name, false);
        }

        clipsRemaining = audioSources.Count();
        clipsScanned = 0;

        UpdateRemainingClipsText();
    }



    void Start()
    {
        timerOn = true;
    }

    private void Update()
    {
        if (timerOn)
        {
            if (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                UpdateTimerText(timeLeft);
            }
            else
            {
                timeLeft = 0;
                //timerOn = false;
                TimeOver();
            }
        }
    }

    public void PlayByName(string clipToPlay)
    {
        //Debug.Log($"OG Text is: {clipToPlay}");
        //Adopting naming convention that final character image name is the version number
        clipToPlay = clipToPlay.Substring(0, clipToPlay.Length - 1);

        //Debug.Log($"Trimmed Text is: {clipToPlay}");

        if (audioClips.ContainsKey(clipToPlay))
        {
            if (playedClips[clipToPlay] == false)
            {   
                playedClips[clipToPlay] = true;
                clipsScanned++;
                clipsRemaining--;

                UpdateRemainingClipsText();
                audioClips[clipToPlay].Play();
                CheckForEnd();
            }
        }
    }

    void UpdateTimerText(float currentTime)
    {
        currentTime += 1;
        string minutes = Mathf.FloorToInt(currentTime / 60).ToString("00");
        string seconds = Mathf.FloorToInt(currentTime % 60).ToString("00");

        timerText.text = origTimerText + string.Format("{0}:{1}", minutes, seconds);
    }

    void UpdateRemainingClipsText()
    {
        clipsRemainingText.text = origClipsRemainingText + clipsRemaining.ToString();
        clipsScannedText.text = origClipsScannedText + clipsScanned.ToString();
    }

    public void CheckForEnd()
    {
        if (!playedClips.ContainsValue(false))
        {
            TimeOver();
        }
    }

    public void TimeOver()
    {
        StopAllAudio();
        timerOn = false;
        finalAudio.Play();
    }


    public void StopAllAudio()
    {
        foreach (var audioS in audioClips.Values)
        {
            audioS.Stop();
        }
    }

    public void ResetScene()
    {
        playedClips.Clear();
        audioClips.Clear();
    }


}
