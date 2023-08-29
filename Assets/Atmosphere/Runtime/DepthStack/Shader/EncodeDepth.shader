// Encodes relevant camera depth information into a four-channel texture.
// Channel R stores raw depth
// Channel G stores fragment camera view length for raymarched post-processing effects
// Channels B-W store Z-Buffer parameters needed to decode and linearize raw depth

Shader "Hidden/EncodeDepth"
{

HLSLINCLUDE

#include "../Includes/CompositeDepth.hlsl"

struct appdata 
{
	float4 vertex : POSITION;
	float4 uv : TEXCOORD0;
};


struct v2f 
{
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float3 viewVector : TEXCOORD1;
};


v2f EncodeDepthVertex(appdata v) 
{
	v2f output;
	output.pos = TransformObjectToHClip(v.vertex.xyz);
	output.uv = v.uv.xy;

	// Get view vector for fragment shader - do not normalize here as interpolation will mess it up, only normalize in fragment shader
	float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv.xy * 2 - 1, 0, -1)).xyz;
	output.viewVector = mul(unity_CameraToWorld, float4(viewVector, 0)).xyz;
	return output;
}


float4 EncodeDepthFragment(v2f i) : SV_Target 
{
    // x: raw depth
	// y: view length
	// z-w: z-buffer parameters
	float4 encodedInfo = (float4)0;

	encodedInfo.x = SampleDepth(i.uv);
	encodedInfo.y = length(i.viewVector);
	encodedInfo.z = _ZBufferParams.z;
	encodedInfo.w = _ZBufferParams.w;

    return encodedInfo;
}

ENDHLSL

    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "EncodeDepth"

            HLSLPROGRAM
            #pragma vertex EncodeDepthVertex
            #pragma fragment EncodeDepthFragment

            ENDHLSL
        }
    }
}
