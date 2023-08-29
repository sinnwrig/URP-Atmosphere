Shader "Custom/Glow"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _Glow ("Glow", Float) = 2.0
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {   
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag


            #include "/Includes/Common.hlsl"


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };


            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            half4 _MainColor;
            half _Glow;


            v2f vert (appdata v)
            {
                v2f output;
	            output.vertex = TransformObjectToHClip(v.vertex.xyz);
	            output.uv = v.uv.xy;
	            return output;
            }

            half4 frag (v2f i) : SV_Target
            {
                // sample the texture
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                return col * _MainColor * _Glow;
            }

            ENDHLSL
        }
    }
}
