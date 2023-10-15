using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


[CustomEditor(typeof(AtmosphereEffect))]
public class AtmosphereEffectEditor : Editor
{
    SerializedProperty profile;
    SerializedProperty sun;
    SerializedProperty directional;
    SerializedProperty planetRadius;
    SerializedProperty cutoffDepth;
    SerializedProperty atmosphereScale;


    void OnEnable()
    {
        profile = serializedObject.FindProperty("profile");
        sun = serializedObject.FindProperty("sun");
        directional = serializedObject.FindProperty("directional");
        planetRadius = serializedObject.FindProperty("planetRadius");
        cutoffDepth = serializedObject.FindProperty("cutoffDepth");
        atmosphereScale = serializedObject.FindProperty("atmosphereScale");
    }



    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        ShowWarnings();

        DrawPropertyLabelControl(profile, new GUIContent("Profile", "The Atmosphere Profile used for rendering the Atmosphere Effect."), GUILayout.Width(110));

        GUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(new GUIContent("Sun", "The main light that affects the atmosphere."), GUILayout.Width(110));
        EditorGUILayout.PropertyField(sun, GUIContent.none);

        TightLabel("Directional", "How the atmosphere should treat light direction. If Directional is enabled, atmosphere will treat light direction as applying evenly everywhere. Otherwise, shader will calculate light direction based on direction to sun transform.");
        EditorGUILayout.PropertyField(directional, GUIContent.none, GUILayout.Width(15));

        GUILayout.EndHorizontal();

        float prevWidth = EditorGUIUtility.labelWidth;

        EditorGUIUtility.labelWidth = 110;

        EditorGUILayout.PropertyField(planetRadius, new GUIContent("Planet Radius", "The radius of the planet the atmosphere renders above."));
        EditorGUILayout.PropertyField(cutoffDepth, new GUIContent("Cutoff Depth", "The depth below the Planet Radius in which the atmosphere will no longer render. Change this value to prevent flickering or infinitely bright objects at the planet center"));
        EditorGUILayout.PropertyField(atmosphereScale, new GUIContent("Atmosphere Scale", "The scale of the planet atmosphere relative to the planet radius"));

        EditorGUIUtility.labelWidth = prevWidth;

        serializedObject.ApplyModifiedProperties();
    }


    void ShowWarnings()
    {
        AtmosphereEffect effect = (AtmosphereEffect)target;

        if (!(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset))
        {
            EditorGUILayout.HelpBox("Effect is only compatible with the Universal Render Pipeline!", MessageType.Error); 
        }
        else if (effect.profile == null)
        {
            EditorGUILayout.HelpBox("Atmosphere Profile required to display effect!", MessageType.Error); 
        }
        else if (effect.sun == null)
        {
            EditorGUILayout.HelpBox("Sun transform required to display effect!", MessageType.Error); 
        }
    }


    public void OnSceneGUI()
    {
        AtmosphereEffect[] effects = System.Array.ConvertAll(targets, item => (AtmosphereEffect)item);

        for (int i = 0; i < effects.Length; i++) 
        {   
            var effect = effects[i];

            EditorGUI.BeginChangeCheck();
            Handles.color = Color.yellow;
            float newPlanet = Handles.RadiusHandle(Quaternion.identity, effect.transform.position, effect.planetRadius);

            Handles.color = Color.red;
            float newCutoff = Handles.RadiusHandle(Quaternion.identity, effect.transform.position, -effect.cutoffDepth + effect.planetRadius);

            Handles.color = Color.blue;
		    float newAtmo = Handles.RadiusHandle(Quaternion.identity, effect.transform.position, effect.AtmosphereSize);


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(effect, "Changed Atmosphere Radii");

                effect.atmosphereScale = (newAtmo / effect.planetRadius) - 1;
                effect.planetRadius = newPlanet;
                effect.cutoffDepth = -(newCutoff - effect.planetRadius);
            }
        }
    }


    void DrawPropertyLabelControl(SerializedProperty property, GUIContent content, params GUILayoutOption[] options)
    {
        GUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(content, options);
        EditorGUILayout.PropertyField(property, GUIContent.none);

        GUILayout.EndHorizontal();
    }


    public static void TightLabel(string labelStr, string tooltip = null)
    {
        GUIContent label = tooltip == null ? new GUIContent(labelStr) : new GUIContent(labelStr, tooltip);
        EditorGUILayout.LabelField(label, GUILayout.Width(GUI.skin.label.CalcSize(label).x));
    }
}
