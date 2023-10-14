using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(AtmosphereProfile))]
public class AtmosphereProfileEditor : Editor
{
    SerializedProperty textureSize;
    SerializedProperty opticalDepthCompute;
    SerializedProperty opticalDepthPoints;
    SerializedProperty inScatteringPoints;
    SerializedProperty sunIntensity;
    SerializedProperty rayleighScatter;
    SerializedProperty rayleighDensityFalloff;
    SerializedProperty mieScatter;
    SerializedProperty mieDensityFalloff;
    SerializedProperty mieG;
    SerializedProperty heightAbsorbtion;
    SerializedProperty absorbtionColor;
    SerializedProperty ambientColor;


    void OnEnable()
    {
        textureSize = serializedObject.FindProperty("textureSize");
        opticalDepthCompute = serializedObject.FindProperty("opticalDepthCompute");
        opticalDepthPoints = serializedObject.FindProperty("opticalDepthPoints");

        inScatteringPoints = serializedObject.FindProperty("inScatteringPoints");
        sunIntensity = serializedObject.FindProperty("sunIntensity");

        rayleighScatter = serializedObject.FindProperty("rayleighScatter");
        rayleighDensityFalloff = serializedObject.FindProperty("rayleighDensityFalloff");

        mieScatter = serializedObject.FindProperty("mieScatter");
        mieDensityFalloff = serializedObject.FindProperty("mieDensityFalloff");
        mieG = serializedObject.FindProperty("mieG");

        heightAbsorbtion = serializedObject.FindProperty("heightAbsorbtion");
        absorbtionColor = serializedObject.FindProperty("absorbtionColor");

        ambientColor = serializedObject.FindProperty("ambientColor");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUILayout.BeginVertical("GroupBox");
        EditorGUILayout.LabelField("Optical Depth Baking", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(textureSize);
        EditorGUILayout.PropertyField(opticalDepthCompute);
        EditorGUILayout.PropertyField(opticalDepthPoints);
        GUILayout.EndVertical();

        GUILayout.BeginVertical("GroupBox");
        EditorGUILayout.LabelField("Lighting", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(inScatteringPoints);
        EditorGUILayout.PropertyField(sunIntensity);
        EditorGUILayout.PropertyField(ambientColor);
        GUILayout.EndVertical();

        GUILayout.BeginVertical("GroupBox");
        EditorGUILayout.LabelField("Rayleigh Scattering", EditorStyles.boldLabel);
        DrawWavelengthProperty(rayleighScatter);
        EditorGUILayout.PropertyField(rayleighDensityFalloff);
        GUILayout.EndVertical();

        GUILayout.BeginVertical("GroupBox");
        EditorGUILayout.LabelField("Mie Scattering", EditorStyles.boldLabel);
        DrawWavelengthProperty(mieScatter);
        EditorGUILayout.PropertyField(mieDensityFalloff);
        EditorGUILayout.PropertyField(mieG);
        GUILayout.EndVertical();

        GUILayout.BeginVertical("GroupBox");
        EditorGUILayout.LabelField("Ozone Absorbtion", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(heightAbsorbtion);
        EditorGUILayout.PropertyField(absorbtionColor);
        GUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }


    const float colorFactor = 0.25f;


    void DrawWavelengthProperty(SerializedProperty property)
    {
        SerializedProperty red = property.FindPropertyRelative("red");
        SerializedProperty green = property.FindPropertyRelative("green");
        SerializedProperty blue = property.FindPropertyRelative("blue");
        SerializedProperty power = property.FindPropertyRelative("power");

        GUILayout.BeginHorizontal();

        EditorGUIUtility.labelWidth -= 35;
        property.isExpanded = EditorGUILayout.Toggle("Use Color Picker", property.isExpanded);
        EditorGUIUtility.labelWidth += 35;


        if (property.isExpanded) 
        {
            Color col = new Color(red.floatValue, green.floatValue, blue.floatValue) / colorFactor;

            col = EditorGUILayout.ColorField(col);

            red.floatValue = col.r * colorFactor;
            green.floatValue = col.g * colorFactor;
            blue.floatValue = col.b * colorFactor;

            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(red);
            EditorGUILayout.PropertyField(green);
            EditorGUILayout.PropertyField(blue);
        }

        EditorGUILayout.PropertyField(power);
    }
}
