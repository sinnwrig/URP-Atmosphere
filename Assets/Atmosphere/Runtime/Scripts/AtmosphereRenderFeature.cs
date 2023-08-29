using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AtmosphereRenderFeature : ScriptableRendererFeature
{
    private Shader atmosphereShader;

    AtmosphereRenderPass atmospherePass;



    public override void Create() 
    {
        ValidateShader();

        atmospherePass = new AtmosphereRenderPass(atmosphereShader);

        // Effect does not work with transparents since they do not write to the depth buffer. Sorry if you wanted to have a planet made of glass.
        atmospherePass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;

        atmospherePass.ConfigureInput(ScriptableRenderPassInput.Depth);
    }


    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) 
    {
        // Prevent renering in material previews.
        if (!renderingData.cameraData.isPreviewCamera) 
        {
            renderer.EnqueuePass(atmospherePass);
        }
    }


    void ValidateShader() 
    {
        Shader shader = AddAlwaysIncludedShader("Hidden/Atmosphere");

        if (shader == null) 
        {
            Debug.LogError("Atmosphere shader could not be found! Make sure Hidden/Atmosphere is located somewhere in your project and included in 'Always Included Shaders'", this);
            return;
        }

        atmosphereShader = shader;
    }


    // NOTE: Does not always immediately add the shader. If the shader was just recently imported with the project, will return null as the shader hasn't compiled yet
    public static Shader AddAlwaysIncludedShader(string shaderName)
    {
        var shader = Shader.Find(shaderName);
        if (shader == null) {
            return null;
        }
     
#if UNITY_EDITOR
        var graphicsSettingsObj = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
        var serializedObject = new SerializedObject(graphicsSettingsObj);
        var arrayProp = serializedObject.FindProperty("m_AlwaysIncludedShaders");
        bool hasShader = false;

        for (int i = 0; i < arrayProp.arraySize; ++i)
        {
            var arrayElem = arrayProp.GetArrayElementAtIndex(i);
            if (shader == arrayElem.objectReferenceValue)
            {
                hasShader = true;
                break;
            }
        }
     
        if (!hasShader)
        {
            int arrayIndex = arrayProp.arraySize;
            arrayProp.InsertArrayElementAtIndex(arrayIndex);
            var arrayElem = arrayProp.GetArrayElementAtIndex(arrayIndex);
            arrayElem.objectReferenceValue = shader;
     
            serializedObject.ApplyModifiedProperties();
     
            AssetDatabase.SaveAssets();
        }
#endif

        return shader;
    }
}


