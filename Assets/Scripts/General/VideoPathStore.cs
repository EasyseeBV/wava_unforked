using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class VideoPathData
{
    public List<string> paths = new();
}

/// <summary>
/// Stores the specified paths in a JSON file and can retrieve them again.
/// </summary>
public static class VideoPathStore
{
    public static readonly string FileName = "video_paths.json";
    public static readonly string FilePath = Path.Combine(Application.persistentDataPath, FileName);

    // Store a new video path.
    public static void StorePath(string path)
    {
        var data = LoadOrCreateData();
        if (!data.paths.Contains(path))
        {
            data.paths.Add(path);
            Save(data);
        }
    }

    // Read all stored video paths.
    public static List<string> ReadPaths()
    {
        return LoadOrCreateData().paths;
    }

    // Remove a path.
    public static void RemovePath(string path)
    {
        var data = LoadOrCreateData();
        if (data.paths.Remove(path))
        {
            Save(data);
        }
    }

    public static string ConvertToMediaStoreURIIfAlias(string path)
    {
        return IsMediaStoreAlias(path) ? ConvertAliasToMediaStoreURI(path) : path;
    }

    static bool IsMediaStoreAlias(string path)
    {
       return path.Contains("/video/media/") && (path.StartsWith("/external_primary/") || path.StartsWith("/external/"));
    }

    static string ConvertAliasToMediaStoreURI(string aliasPath)
    {
        var match = System.Text.RegularExpressions.Regex.Match(aliasPath, @"/video/media/(\d+)");

        // If the match is successful, return the full content URI.
        return match.Success ? $"content://media/external/video/media/{match.Groups[1].Value}" : null;
    }

    // Load JSON file or create new.
    static VideoPathData LoadOrCreateData()
    {
        if (File.Exists(FilePath))
        {
            string json = File.ReadAllText(FilePath);
            return JsonUtility.FromJson<VideoPathData>(json) ?? new VideoPathData();
        }
        return new VideoPathData();
    }

    // Save to JSON file.
    static void Save(VideoPathData data)
    {
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(FilePath, json);
    }
}