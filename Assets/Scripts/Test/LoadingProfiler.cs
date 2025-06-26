using System.Diagnostics;

public class LoadingProfiler
{
    private Stopwatch stopwatch = new Stopwatch();

    public void StartTracking()
    {
        stopwatch.Reset();
        stopwatch.Start();
    }

    public void LogStep(string stepName)
    {
        stopwatch.Stop();
        UnityEngine.Debug.Log($"{stepName} took {stopwatch.ElapsedMilliseconds} ms");
        stopwatch.Reset();
        stopwatch.Start();
    }

    public void EndTracking()
    {
        stopwatch.Stop();
        UnityEngine.Debug.Log($"Total loading time: {stopwatch.ElapsedMilliseconds} ms");
    }
}