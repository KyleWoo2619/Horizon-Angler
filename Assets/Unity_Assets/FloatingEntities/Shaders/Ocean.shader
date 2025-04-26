Shader "URP/OceanAdvancedFresnel"
{
    Properties
    {
        _WaveLengthInverse ("Wave Length Inverse (1/WaveSize)", Float) = 10.0
        _Intensity ("Wave Intensity", Float) = 4.0
        _Periode ("Wave Period", Float) = 1.0
        _WaveRotation ("Wave Rotation (Degrees)", Range(0, 360)) = 0

        _UseSecondWave ("Use Second Wave (0=Off, 1=On)", Float) = 0
        _WaveLengthInverse2 ("Wave2 Length Inverse", Float) = 5.0
        _Intensity2 ("Wave2 Intensity", Float) = 2.0
        _Periode2 ("Wave2 Period", Float) = 0.5
        _WaveRotation2 ("Wave2 Rotation (Degrees)", Range(0, 360)) = 45

        _FoamTex ("Foam Texture", 2D) = "white" {}
        _FoamDensity ("Foam Density", Range(0.01, 1)) = 0.1
        _FoamStrength ("Foam Strength", Range(0, 1)) = 0.15
        _FoamSpeed ("Foam Scroll Speed", Float) = 0.5
        _FoamDirection ("Foam Move Direction", Vector) = (1, 0, 0, 0)

        _BumpMap ("Normals", 2D) = "bump" {}
        _BumpTiling ("Bump Tiling", Vector) = (1,1,-2,3)
        _BumpDirection ("Bump Direction & Speed", Vector) = (1,1,-1,1)
        _FresnelScale ("Fresnel Scale", Range (0.15, 4.0)) = 0.75
        _BaseColor ("Base color", Color) = (0.54, 0.95, 0.99, 0.5)
        _ReflectionColor ("Reflection color", Color) = (0.54, 0.95, 0.99, 0.5)
        _SpecularColor ("Specular color", Color) = (0.72, 0.72, 0.72, 1)
        _Shininess ("Shininess", Range(2,500)) = 200
        _DistortParams ("Distortions (waves, reflection, fresnel power, bias)", Vector) = (1,1,2,1.15)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 300

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                float3 worldPosition : TEXCOORD1;
                float3 viewVector : TEXCOORD2;
                float2 bumpUV1 : TEXCOORD3;
                float2 bumpUV2 : TEXCOORD4;
                float fogFactor : TEXCOORD6;
            };

            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
            TEXTURE2D(_FoamTex); SAMPLER(sampler_FoamTex);

            float _WaveLengthInverse, _Intensity, _Periode, _WaveRotation;
            float _UseSecondWave, _WaveLengthInverse2, _Intensity2, _Periode2, _WaveRotation2;

            float _FoamDensity, _FoamStrength, _FoamSpeed;
            float4 _FoamDirection;
            float4 _BaseColor, _ReflectionColor, _SpecularColor;
            float4 _BumpDirection, _BumpTiling, _DistortParams;
            float _Shininess, _FresnelScale;

            float waterHeight(float waveLen, float intensity, float period, float2 pos)
            {
                float time = _Time.y;
                float angle = radians(_WaveRotation);
                float2 dir = float2(cos(angle), sin(angle));
                float height = sin(dot(pos, dir * waveLen) + time * period) * intensity;

                if (_UseSecondWave > 0.5)
                {
                    float angle2 = radians(_WaveRotation2);
                    float2 dir2 = float2(cos(angle2), sin(angle2));
                    height += sin(dot(pos, dir2 * _WaveLengthInverse2) + time * _Periode2) * _Intensity2;
                }

                return height;
            }

            float Fresnel(float3 viewDir, float3 normal, float bias, float power)
            {
                return bias + (1.0 - bias) * pow(1.0 - saturate(dot(viewDir, normal)), power);
            }

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                float heightOffset = waterHeight(_WaveLengthInverse, _Intensity, _Periode, worldPos.xz);
                worldPos.y += heightOffset;

                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                OUT.worldPosition = worldPos;
                OUT.viewVector = _WorldSpaceCameraPos - worldPos;

                float2 uv = worldPos.xz;
                OUT.bumpUV1 = (uv + _Time.x * _BumpDirection.xy) * _BumpTiling.xy;
                OUT.bumpUV2 = (uv + _Time.x * _BumpDirection.zw) * _BumpTiling.zw;

                OUT.fogFactor = ComputeFogFactor(OUT.positionHCS.z);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float3 viewDir = normalize(IN.viewVector);

                // Sample normal map in frag (not vert!)
                float3 n1 = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.bumpUV1));
                float3 n2 = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.bumpUV2));
                float3 normalWS = normalize(n1 + n2 + float3(0, 1, 0));
                normalWS = abs(normalWS);

                float3 lightAccum = float3(0,0,0);
                float3 specAccum = float3(0,0,0);

                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float diff = saturate(dot(normalWS, lightDir));
                float3 halfVec = normalize(lightDir + viewDir);
                float spec = pow(saturate(dot(normalWS, halfVec)), _Shininess);

                lightAccum += diff * mainLight.color;
                specAccum += spec * mainLight.color;

                #ifdef _ADDITIONAL_LIGHTS
                uint count = GetAdditionalLightsCount();
                for (uint i = 0; i < count; i++) {
                    Light l = GetAdditionalLight(i, IN.worldPosition);
                    float3 dir = normalize(l.direction);
                    float atten = l.distanceAttenuation;
                    float d = saturate(dot(normalWS, dir));
                    float s = pow(saturate(dot(normalWS, normalize(dir + viewDir))), _Shininess);
                    lightAccum += d * l.color * atten;
                    specAccum += s * l.color * atten;
                }
                #endif

                float fresnel = Fresnel(viewDir, normalWS * _FresnelScale, _DistortParams.w, _DistortParams.z);

                float2 foamUV = IN.worldPosition.xz * _FoamDensity + (_Time.x * _FoamDirection.xy * _FoamSpeed);
                float foam = SAMPLE_TEXTURE2D(_FoamTex, sampler_FoamTex, foamUV).r;

                float3 base = _BaseColor.rgb;
                float3 mixed = lerp(base, _ReflectionColor.rgb, fresnel);
                mixed += foam * _FoamStrength;
                mixed += specAccum * _SpecularColor.rgb;
                mixed *= lightAccum;

                mixed = MixFog(mixed, IN.fogFactor);
                return float4(mixed, 1);
            }
            ENDHLSL
        }
    }
    FallBack Off
}