Shader "Hidden/Atmosphere"
{

HLSLINCLUDE

#include "/Includes/Common.hlsl"
#include "/Includes/Math.hlsl"

#include "../DepthStack/Includes/CompositeDepth.hlsl"
#include "/Includes/Atmosphere.hlsl"


ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Off

		Pass
		{
			Name "RenderAtmosphere"

			HLSLPROGRAM

			#pragma vertex AtmosphereVertex
			#pragma fragment AtmosphereFragment

			#pragma target 4.0
			
			#define ATMOSPHERE_MODEL_SIM

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


			TEXTURE2D(_Source);
			SAMPLER(sampler_Source);


			v2f AtmosphereVertex(appdata v) 
			{
				v2f output;
				output.pos = TransformObjectToHClip(v.vertex.xyz);
				output.uv = v.uv.xy;
				float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv.xy * 2 - 1, 0, -1)).xyz;
				output.viewVector = mul(unity_CameraToWorld, float4(viewVector, 0)).xyz;
				return output;
			}


			float4 AtmosphereFragment(v2f i) : SV_Target 
			{ 
				float4 originalCol = SAMPLE_TEXTURE2D(_Source, sampler_Source, i.uv);

				float viewLength = length(i.viewVector);

				bool isEndOfDepth;
				float sceneDepth = CompositeDepthScaled(i.uv, viewLength, isEndOfDepth);

				float3 color = CalculateScattering(_WorldSpaceCameraPos.xyz, i.viewVector / viewLength, sceneDepth, originalCol.xyz);

				return float4(color, originalCol.w);
			}

			ENDHLSL
		}
	}
}
