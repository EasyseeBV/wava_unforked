using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FrameRateManager
{
    public static void SetHighFrameRate() => Application.targetFrameRate = 60;
    public static void SetDefaultFrameRate() => Application.targetFrameRate = -1;
}
