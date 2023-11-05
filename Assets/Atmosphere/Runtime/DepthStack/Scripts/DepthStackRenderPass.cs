using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


// TODO: Shader is wasting tons of memory as it stores the global _ZBufferParams in the texture channels. Find a way to store this as a single global property.

public class DepthStackRenderPass : ScriptableRenderPass
{
    private Material copyDepth;


    static readonly int encodedDepthTexture = Shader.PropertyToID("_PrevCameraDepth");
    RenderTargetIdentifier encodedDepthTarget = new RenderTargetIdentifier(encodedDepthTexture);
    public static Vector4 prevZBuffer;
    public static Vector4 zClone;



    public DepthStackRenderPass(Material copyDepth) 
    {
        this.copyDepth = copyDepth;
    }



    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var camData = renderingData.cameraData;

        cmd.SetGlobalInt("_RenderOverlay", 0);

        if (!camData.isSceneViewCamera)
        {
            bool isOverlay = camData.camera.GetUniversalAdditionalCameraData().renderType == CameraRenderType.Overlay;

            // Tell shader whether or not to use the Depth Stack
            cmd.SetGlobalInt("_RenderOverlay", isOverlay ? 1 : 0);
            return;
        }
    }
    

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) 
    {
        if (copyDepth == null) 
            return;

        CommandBuffer cmd = CommandBufferPool.Get("Encode Depth");
        cmd.Clear();

        var cameraData = renderingData.cameraData;

        if (!cameraData.isSceneViewCamera)
        {
            bool isOverlay = cameraData.camera.GetUniversalAdditionalCameraData().renderType == CameraRenderType.Overlay;

            // Copy base camera depth
            if (!isOverlay)
            {
                CopyAndEncodeDepth(cmd, cameraData.cameraTargetDescriptor); 

                float near = cameraData.camera.nearClipPlane;
                float far = cameraData.camera.farClipPlane;
                prevZBuffer = GetZBuffParams(far, near);

                cmd.SetGlobalVector("_PrevZBuffer", prevZBuffer);
            }
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


    public static Vector4 GetZBuffParams(float far, float near)
    {
        float x = 1-far/near;
        float y = far/near;

        return new Vector4(x, y, x/far, y/far);
    }
}