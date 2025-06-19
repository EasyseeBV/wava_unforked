using System;
using System.Collections;
using UnityEngine;

public static class CoroutineExtensions
{
    public static void InvokeNextFrame(this MonoBehaviour mb, Action callback)
    {
        mb.StartCoroutine(InvokeRoutine(callback));
    }

    private static IEnumerator InvokeRoutine(Action callback)
    {
        yield return null;
        callback?.Invoke();
    }
}
