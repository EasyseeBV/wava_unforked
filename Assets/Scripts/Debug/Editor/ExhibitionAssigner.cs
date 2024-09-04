using System.Collections;
using System.Collections.Generic;
using Messy.Definitions;
using UnityEditor;
using UnityEngine;

public class ExhibitionAssigner : MonoBehaviour
{
    [MenuItem("Tools/Assign Exhibitions")]
    private static void AssignExhibitions()
    {
        ARStaticInfo arStaticInfo = ARStaticInfo.Instance;
        if (arStaticInfo == null)
        {
            Debug.LogError("ARStaticInfo instance not found. Ensure it is placed in a Resources folder and named 'ARStaticInfo'.");
            return;
        }

        // Find all instances of ExhibitionSO in the project
        string[] guids = AssetDatabase.FindAssets("t:ExhibitionSO");
        List<ExhibitionSO> exhibitions = new List<ExhibitionSO>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ExhibitionSO exhibition = AssetDatabase.LoadAssetAtPath<ExhibitionSO>(path);
            if (exhibition != null)
            {
                exhibitions.Add(exhibition);
            }
        }

        // Assign the found ExhibitionSOs to the ARStaticInfo's list
        arStaticInfo.Exhibitions = exhibitions;

        // Mark the ARStaticInfo as dirty so the changes are saved
        EditorUtility.SetDirty(arStaticInfo);

        Debug.Log($"{exhibitions.Count} ExhibitionSO(s) assigned to ARStaticInfo.");
    }
}
