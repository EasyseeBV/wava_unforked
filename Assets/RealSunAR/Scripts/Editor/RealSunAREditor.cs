using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RealSunAR))]
public class RealSunAREditor : Editor
{
    //VERSION 2.10

    //Serialized Properties
    SerializedProperty anglesWhichInterpolate_Serialized;
    SerializedProperty perpendicularSunColor_Serialized;
    SerializedProperty grazedSunColor_Serialized;
    SerializedProperty locationCanvas_Serialized;
    SerializedProperty locationCanvasDuration_Serialized;
    SerializedProperty myWeather_Serialized;
    SerializedProperty weatherAPI_ID_Serialized;
    SerializedProperty shadowMIN_Serialized;
    SerializedProperty shadowMAX_Serialized;

    private void OnEnable()
    {
        anglesWhichInterpolate_Serialized = serializedObject.FindProperty("anglesWhichInterpolate");
        perpendicularSunColor_Serialized = serializedObject.FindProperty("perpendicularSunColor");
        grazedSunColor_Serialized = serializedObject.FindProperty("grazedSunColor");
        locationCanvas_Serialized = serializedObject.FindProperty("locationCanvas");
        locationCanvasDuration_Serialized = serializedObject.FindProperty("locationCanvasDuration");
        myWeather_Serialized = serializedObject.FindProperty("myWeather");
        weatherAPI_ID_Serialized = serializedObject.FindProperty("weatherAPI_ID");
        shadowMIN_Serialized = serializedObject.FindProperty("shadowMIN");
        shadowMAX_Serialized = serializedObject.FindProperty("shadowMAX");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var typeRect = new Rect();
        RealSunAR myTarget = (RealSunAR)target;

        //WARNINGS
        if (QualitySettings.shadows == ShadowQuality.Disable)
        {
            if (myTarget.enableConsoleWarnings) Debug.LogError("RealSunAR: <b>Realtime Shadows are currently dissabled</b>. Please go to Project Settings -> Quality and set Shadows to something other than Disabled");
            EditorGUILayout.HelpBox("Realtime Shadows are currently dissabled.\nPlease go to Project Settings -> Quality and set Shadows to something other than Disabled", MessageType.Error);
        }

        if (QualitySettings.shadowProjection == ShadowProjection.CloseFit)
        {
            if (myTarget.enableConsoleWarnings) Debug.LogWarning("RealSunAR: Shadow Projection is currently set to <b>Close Fit</b>. In case of shadow flickering, try going to Project Settings -> Quality and change it to <b>Stable Fit</b>.\nIt might make the shadow quality a little lower but it should make them more stable.");
            EditorGUILayout.HelpBox("Shadow Projection is currently set to Close Fit. In case of shadow flickering, try going to Project Settings -> Quality and change it to Stable Fit.\nIt might make the shadow quality a little lower but it should make them more stable.", MessageType.Info);
        }
        if (myTarget.enableConsoleWarnings && EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android) Debug.Log("RealSunAR TIP: Feel safe to go into Player Settings -> Other Settings and turn on <b>Low Accuracy Location</b>");

        //VERSION
        EditorGUILayout.HelpBox("Version 2.10", MessageType.None);

        //Draw the inspector as asked by the script and bellow that begin to draw elements depending on choices
        DrawDefaultInspector();

        //INTERPOLATIONS
        if (myTarget.intensityInterpolation || myTarget.colorInterpolation)
        {
            //myTarget.anglesWhichInterpolate = EditorGUILayout.Slider("Altitude angles to interpolate", myTarget.anglesWhichInterpolate, 0f, 90f);
            //***          
            anglesWhichInterpolate_Serialized.floatValue = EditorGUILayout.Slider("Altitude degrees to interpolate", anglesWhichInterpolate_Serialized.floatValue, 0f, 90f);
            typeRect = GUILayoutUtility.GetLastRect();
            GUI.Label(typeRect, new GUIContent("", "Use this slider to set the altitude degrees during which interpolation occurs"));
            if (myTarget.colorInterpolation)
            {
                //***myTarget.perpendicularSunColor = EditorGUILayout.ColorField("Perpendicular sun color", myTarget.perpendicularSunColor);
                perpendicularSunColor_Serialized.colorValue = EditorGUILayout.ColorField("Perpendicular sun color", perpendicularSunColor_Serialized.colorValue);
                typeRect = GUILayoutUtility.GetLastRect();
                GUI.Label(typeRect, new GUIContent("", "Sun color when the sun is on it's Zenith"));
                //***myTarget.grazedSunColor = EditorGUILayout.ColorField("Dawn/Dusk sun color", myTarget.grazedSunColor);
                grazedSunColor_Serialized.colorValue = EditorGUILayout.ColorField("Dawn/Dusk sun color", grazedSunColor_Serialized.colorValue);
                typeRect = GUILayoutUtility.GetLastRect();
                GUI.Label(typeRect, new GUIContent("", "Sun color when the sun is at horizon level"));
            }
        }

        //LOCATION SERVICES FIELD
        locationCanvas_Serialized.objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Location warning gameobject", locationCanvas_Serialized.objectReferenceValue, typeof(GameObject), true);
        typeRect = GUILayoutUtility.GetLastRect();
        GUI.Label(typeRect, new GUIContent("", "Activate this gameobject to notify the user to enable location services"));

        if (myTarget.locationCanvas != null)
        {
            locationCanvasDuration_Serialized.intValue = EditorGUILayout.IntField("Location warning duration", locationCanvasDuration_Serialized.intValue);
            typeRect = GUILayoutUtility.GetLastRect();
            GUI.Label(typeRect, new GUIContent("", "How many seconds will this gameobject above be shown before deactivated?"));
        }

        //WEATHER API
        //myWeatherSerialized = (RealSunAR.enumWeatherAPI)EditorGUILayout.PropertyField(RealSunAR.enumWeatherAPI);***
        EditorGUILayout.PropertyField(myWeather_Serialized);
        //myWeatherSerialized.intValue = (RealSunAR.enumWeatherAPI)EditorGUILayout.EnumPopup("Select a weather API service", myTarget.myWeather);***
        typeRect = GUILayoutUtility.GetLastRect();
        GUI.Label(typeRect, new GUIContent("", "Select None to not use weather API or select a weather API (Requires Internet access) to have shadows be influenced by the local weather"));
        if (myTarget.myWeather == RealSunAR.enumWeatherAPI.OpenWeatherMap)
            {
            weatherAPI_ID_Serialized.stringValue = EditorGUILayout.TextField("OpenWeatherMap ID", weatherAPI_ID_Serialized.stringValue);
            typeRect = GUILayoutUtility.GetLastRect();
            GUI.Label(typeRect, new GUIContent("", "Please type your OpenWeatherMap ID here"));
        }
        if (myTarget.myWeather != RealSunAR.enumWeatherAPI.none)
        {
            
            shadowMIN_Serialized.floatValue = EditorGUILayout.Slider("Minimum shadow strength", shadowMIN_Serialized.floatValue, 0f, 1f);
            typeRect = GUILayoutUtility.GetLastRect();
            GUI.Label(typeRect, new GUIContent("", "Amount of shadow strength on 100% cloudiness"));

            shadowMAX_Serialized.floatValue = EditorGUILayout.Slider("Minimum shadow strength", shadowMAX_Serialized.floatValue, 0f, 1f);
            typeRect = GUILayoutUtility.GetLastRect();
            GUI.Label(typeRect, new GUIContent("", "Amount of shadow strength on 0% cloudiness"));   
        }

        serializedObject.ApplyModifiedProperties();
    }
}