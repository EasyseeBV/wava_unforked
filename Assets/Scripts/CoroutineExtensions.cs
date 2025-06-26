using System;
using System.Collections;
using UnityEngine;

public static class CoroutineExtensions
{
    public static void InvokeNextFrame(this MonoBehaviour mb, Action callback)
    {
        mb.StartCoroutine(InvokeRoutine(1, callback));
    }

    public static void InvokeAfterDelay(this MonoBehaviour mb, int waitTimeInFrames, Action callback)
    {
        mb.StartCoroutine(InvokeRoutine(waitTimeInFrames, callback));
    }

    private static IEnumerator InvokeRoutine(int waitTimeInFrames, Action callback)
    {
        for (int i = 0; i < waitTimeInFrames; i++)
        {
            yield return null;
        }

        callback?.Invoke();
    }
}
