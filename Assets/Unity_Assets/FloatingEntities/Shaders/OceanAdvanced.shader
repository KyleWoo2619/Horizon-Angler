// Fixed float3 -> float4 mismatch and pink material bug in URP version
// Ensure URP asset has Opaque Texture enabled

Shader "URP/OceanAdvancedURP"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.54, 0.95, 0.99, 0.5)
        _WaterColor ("Water Color", Color) = (0.54, 0.95, 0.99, 0.5)
        _ReflectionColor ("Reflection Color", Color) = (0.54, 0.95, 0.99, 0.5)
        _SpecularColor ("Specular Color", Color) = (0.72, 0.72, 0.72, 1)
        [NoScaleOffset] _Foam ("Foam Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 300
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPosition : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float4 positionWS : TEXCOORD2;
                float fogFactor : TEXCOORD3;
            };

            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            float4 _BaseColor;
            float4 _WaterColor;
            float4 _ReflectionColor;
            float4 _SpecularColor;
            float4 _Foam_ST;
            TEXTURE2D(_Foam);
            SAMPLER(sampler_Foam);

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionWS = float4(worldPos, 1.0);
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.worldPosition = worldPos;
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                OUT.fogFactor = ComputeFogFactor(OUT.positionHCS.z);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float3 sceneColor = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV).rgb;

                float foamFactor = SAMPLE_TEXTURE2D(_Foam, sampler_Foam, IN.worldPosition.xz * 0.1).r;
                float3 foamColor = _WaterColor.rgb + foamFactor * 0.3;
                float3 finalColor = lerp(sceneColor, foamColor, foamFactor);

                half4 color;
                color.rgb = finalColor;
                color.a = 1;

                color.rgb = MixFog(color.rgb, IN.fogFactor);
                return color;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
