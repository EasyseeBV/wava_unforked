using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.Networking;

[CustomEditor(typeof(RealSunARTester))]
public class RealSunARTesterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var myScript = (RealSunARTester)target;

        EditorGUILayout.HelpBox("Tester Version " + myScript.version, MessageType.None);

        //Draw inspector without any changes
        DrawDefaultInspector();

        if (GUILayout.Button("Check for update"))
        {                   
            myScript.CheckUpdates();            
        }

        if (myScript.readingUpdate == true) EditorGUILayout.HelpBox("Check results on the console", MessageType.Info);
    }




}
