using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class DepthStackRenderPass : ScriptableRenderPass
{
    private Material copyDepth;


    static readonly int encodedDepthTexture = Shader.PropertyToID("_PrevCameraDepth");
    RenderTargetIdentifier encodedDepthTarget = new RenderTargetIdentifier(encodedDepthTexture);



    public DepthStackRenderPass(Material copyDepth) 
    {
        this.copyDepth = copyDepth;
    }


    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) 
    {
        var cameraData = renderingData.cameraData;
        Camera camera = cameraData.camera;
        bool isOverlay = camera.GetUniversalAdditionalCameraData().renderType == CameraRenderType.Overlay && !cameraData.isSceneViewCamera;

        // Tell shader whether or not to use the Depth Stack
        cmd.SetGlobalInteger("_RenderOverlay", (isOverlay ? 1 : 0));
    }
    

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) 
    {
        if (copyDepth == null) 
        {
            return;
        }

        CommandBuffer cmd = CommandBufferPool.Get("Encode Depth");
        cmd.Clear();

        var cameraData = renderingData.cameraData;
        Camera camera = cameraData.camera;

        bool isOverlay = camera.GetUniversalAdditionalCameraData().renderType == CameraRenderType.Overlay;

        if (!isOverlay && !cameraData.isSceneViewCamera) 
        {
            CopyAndEncodeDepth(cmd, cameraData.cameraTargetDescriptor);
        } 

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }


    void CopyAndEncodeDepth(CommandBuffer cmd, RenderTextureDescriptor camDescriptor) 
    {
        // Clone camera descriptor, and ensure 4-channel float usage.
        cmd.ReleaseTemporaryRT(encodedDepthTexture);
        camDescriptor.depthBufferBits = 0;
        camDescriptor.colorFormat = RenderTextureFormat.ARGBFloat;
        cmd.GetTemporaryRT(encodedDepthTexture, camDescriptor, FilterMode.Bilinear);

        // Copy the depth texture into another render texture using a shader pass that encodes relevant information
        cmd.Blit(encodedDepthTarget, encodedDepthTarget, copyDepth);
    }
}