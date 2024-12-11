using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Messy.Definitions;
using UnityEditor;
using UnityEngine;

public class ScriptableObjectDateUpdater : MonoBehaviour
{
    [MenuItem("Tools/Auto Assign SO Creation Dates")]
    public static void AssignDataDates()
    {
        // Auto assign the creation time for all EXHIBITIONS
        string[] exhGuids = AssetDatabase.FindAssets("t:ExhibitionSO");
        int exhibitionsUpdated = 0;
        foreach (string guid in exhGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ExhibitionSO exhibition = AssetDatabase.LoadAssetAtPath<ExhibitionSO>(path);
            if (exhibition != null)
            {
                string absolutePath = Path.Combine(Application.dataPath, path.Substring("Assets/".Length));
                DateTime creationTime = File.GetCreationTime(absolutePath);
                long unixTime = ((DateTimeOffset)creationTime).ToUnixTimeSeconds();
                exhibition.creationDateTime = unixTime;
                EditorUtility.SetDirty(exhibition);
                exhibitionsUpdated++;
            }
        }
        
        // Auto assign the creation time for all ARTWORKS
        string[] artworkGuids = AssetDatabase.FindAssets("t:ARPointSO");
        int artworkUpdated = 0;
        foreach (string guid in artworkGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ARPointSO artwork = AssetDatabase.LoadAssetAtPath<ARPointSO>(path);
            if (artwork != null)
            {
                string absolutePath = Path.Combine(Application.dataPath, path.Substring("Assets/".Length));
                DateTime creationTime = File.GetCreationTime(absolutePath);
                long unixTime = ((DateTimeOffset)creationTime).ToUnixTimeSeconds();
                artwork.creationDateTime = unixTime;
                EditorUtility.SetDirty(artwork);
                artworkUpdated++;
            }
        }
        
        
        // Auto assign the creation time for all ARTISTS
        string[] artistGuids = AssetDatabase.FindAssets("t:ArtistSO");
        int artistUpdated = 0;
        foreach (string guid in artistGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ArtistSO artist = AssetDatabase.LoadAssetAtPath<ArtistSO>(path);
            if (artist != null)
            {
                string absolutePath = Path.Combine(Application.dataPath, path.Substring("Assets/".Length));
                DateTime creationTime = File.GetCreationTime(absolutePath);
                long unixTime = ((DateTimeOffset)creationTime).ToUnixTimeSeconds();
                artist.creationDateTime = unixTime;
                EditorUtility.SetDirty(artist);
                artistUpdated++;
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"Completed auto assigning of creation dates to data. {exhibitionsUpdated + artworkUpdated + artistUpdated} entries updated. | Exhibitions [{exhibitionsUpdated}] | Artworks [{artworkUpdated}] | Artists [{artistUpdated}]");
    }
}
