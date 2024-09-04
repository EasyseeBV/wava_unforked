using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StartScreenManager : MonoBehaviour
{
    public UnityEvent events;
    static public bool Once;

    // Start is called before the first frame update
    void Start()
    {
        if (Once)
            InvokeClick();
    }

    public void InvokeClick() {
        events.Invoke();
        Once = true;
    }
}
