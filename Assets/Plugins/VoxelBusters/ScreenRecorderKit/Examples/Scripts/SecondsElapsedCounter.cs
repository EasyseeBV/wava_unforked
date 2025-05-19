using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SecondsElapsedCounter : MonoBehaviour
{
    [SerializeField]
    private Text m_text;

    private float m_startTime;

    private void Awake()
    {
        Activate(false);
    }

    // Update is called once per frame
    private void Update()
    {
        m_text.text = ((int)(Time.realtimeSinceStartup - m_startTime)).ToString();
    }

    public void StartTimer()
    {
        m_startTime = Time.realtimeSinceStartup;
        Activate(true);
    }

    public void StopTimer()
    {
        Activate(false);
    }

    public void Activate(bool active)
    {
        gameObject.SetActive(active);
    }
}
