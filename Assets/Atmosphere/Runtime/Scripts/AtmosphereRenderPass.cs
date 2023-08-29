using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using UnityEditor;


#if UNITY_EDITOR
using System.Reflection;
using UnityEditor.SceneManagement;
#endif

public class AtmosphereRenderPass : ScriptableRenderPass
{
    struct SortedEffect
    {
        public AtmosphereEffect effect;
        public float distanceToEffect;
    }


    private static Shader atmosphereShader;


    public AtmosphereRenderPass(Shader atmosphereShader) 
    {
        AtmosphereRenderPass.atmosphereShader = atmosphereShader;
    }


    private static readonly List<AtmosphereEffect> currentActiveEffects = new();


    public static void RegisterEffect(AtmosphereEffect effect) 
    {
        if (!currentActiveEffects.Contains(effect)) 
        {
            currentActiveEffects.Add(effect);
        }
    }


    public static void RemoveEffect(AtmosphereEffect effect) 
    {
        currentActiveEffects.Remove(effect);
    }


    private readonly List<SortedEffect> visibleEffects = new();

    private Plane[] cameraPlanes;


    private void CullAndSortEffects(Camera camera) 
    {
        visibleEffects.Clear();

        // Perform culling of active effects
        cameraPlanes = GeometryUtility.CalculateFrustumPlanes(camera);

        Vector3 viewPos = camera.transform.position;
        for (int i = currentActiveEffects.Count - 1; i >= 0; i--) 
        {
            if (currentActiveEffects[i] == null) 
            {
                currentActiveEffects.RemoveAt(i);
                continue;
            }

            if (currentActiveEffects[i].IsVisible(cameraPlanes)) 
            {
                float dstToSurface = currentActiveEffects[i].DistToAtmosphere(viewPos);

                visibleEffects.Add(new SortedEffect { 
                    effect = currentActiveEffects[i],
                    distanceToEffect = dstToSurface
                });
            }
        }

        // Sort effects from far to near
		for (int i = 0; i < visibleEffects.Count - 1; i++) 
        {
			for (int j = i + 1; j > 0; j--) 
            {
				if (visibleEffects[j - 1].distanceToEffect < visibleEffects[j].distanceToEffect) 
                {
                    // Swap elements
                    (visibleEffects[j], visibleEffects[j - 1]) = (visibleEffects[j - 1], visibleEffects[j]);
                }
            }
		}
    }


    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) 
    {
        BlitUtility.SetupBlitTargets(cmd, renderingData.cameraData.cameraTargetDescriptor);

        CullAndSortEffects(renderingData.cameraData.camera);
    }
    

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) 
    {
        CommandBuffer cmd = CommandBufferPool.Get("Atmosphere Effects");
        cmd.Clear();

        var cameraData = renderingData.cameraData;
        Camera camera = cameraData.camera;

        bool isOverlay = camera.GetUniversalAdditionalCameraData().renderType == CameraRenderType.Overlay;
        

#if UNITY_EDITOR
        // Likely a bug or oversight- scene camera is considered overlay camera when it shouldn't be, so ensure it's not set as overlay.
        isOverlay = !cameraData.isSceneViewCamera && isOverlay;

        bool prefabMode = PrefabStageUtility.GetCurrentPrefabStage() == StageUtility.GetCurrentStage();

        if (cameraData.postProcessEnabled) 
        {
            RenderEffects(cmd, renderingData.cameraData.renderer.cameraColorTargetHandle, prefabMode);
        }
#else
        if (cameraData.postProcessEnabled) 
        {
            RenderEffects(cmd, renderingData.cameraData.renderer.cameraColorTargetHandle, false);
        }
#endif


        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }


    void RenderEffects(CommandBuffer cmd, RenderTargetIdentifier colorSource, bool inPrefabMode) 
    {
        BlitUtility.BeginBlitLoop(cmd, colorSource);

        // Visible effect render loop
        for (int i = 0; i < visibleEffects.Count; i++) 
        {
#if UNITY_EDITOR
            PrefabStage stage = PrefabStageUtility.GetPrefabStage(visibleEffects[i].effect.gameObject);
            if (stage == null && inPrefabMode) 
            {
                continue;
            }
#endif
            Material blitMat = visibleEffects[i].effect.GetMaterial(atmosphereShader);
            BlitUtility.BlitNext(blitMat, "_Source");
        }

        // Blit to camera target
        BlitUtility.EndBlitLoop(colorSource);
    }


    public override void OnCameraCleanup(CommandBuffer cmd) 
    {
        BlitUtility.ReleaseBlitTargets(cmd);
    }
}