#ifndef COMPOSITE_DEPTH_INCLUDED
#define COMPOSITE_DEPTH_INCLUDED

// Include this in a shader to use secondary camera depth

#include "Common.hlsl"

// Camera depth
TEXTURE2D(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);

// Previous camera depth
TEXTURE2D(_PrevCameraDepth);
SAMPLER(sampler_PrevCameraDepth);

int _RenderOverlay;

// Normal, raw depth.
float SampleDepth(float2 uv)
{
    return SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
}

// Previous camera's depth
float4 SamplePrevDepth(float2 uv)
{
    return SAMPLE_TEXTURE2D(_PrevCameraDepth, sampler_PrevCameraDepth, uv);
}


// Composite depth sample- If the current depth sample goes past the depth buffer's bounds, the secondary depth buffer is sampled insted
float4 SampleCompositeDepth(float2 uv) {
	float rawDepth = SampleDepth(uv);
	float4 compositeDepth = 0;

	if (_RenderOverlay == 1 && rawDepth <= 0.0) {
		// If rendering an overlay and end of depth is reached:
		compositeDepth = SamplePrevDepth(uv);
	} else {
		// Normal scene depth
		compositeDepth.x = rawDepth;
		compositeDepth.y = 0;
		compositeDepth.zw = _ZBufferParams.zw;
	}

	return compositeDepth;
}


float CompositeDepthRaw(float2 uv) 
{
	return SampleCompositeDepth(uv).x;
}


float CompositeDepth01(float2 uv) 
{
	float4 compositeDepth = SampleCompositeDepth(uv);
	return Linear01Depth(compositeDepth.x, compositeDepth);
}


float CompositeDepthEye(float2 uv) 
{
    float4 compositeDepth = SampleCompositeDepth(uv);

	return LinearEyeDepth(compositeDepth.x, compositeDepth);
}

// Linear depth scaled by camera view ray distance- useful for finding world position of a fragment or for ray-marching 
float CompositeDepthScaled(float2 uv, float viewLength, out bool isEndOfDepth) 
{
	float rawDepth = SampleDepth(uv);

	isEndOfDepth = rawDepth <= 0.0;

	if (_RenderOverlay == 1 && isEndOfDepth) {
		// If rendering an overlay and end of depth is reached:
		float4 encInfo = SamplePrevDepth(uv);

		isEndOfDepth = encInfo.x <= 0.0;

		return LinearEyeDepth(encInfo.x, encInfo) * encInfo.y;
	}

	return LinearEyeDepth(rawDepth, _ZBufferParams) * viewLength;
}

#endif


