Shader "Custom/WaterWave"
{
    Properties
    {
        _Color ("Water Color", Color) = (0.2, 0.5, 0.8, 1)
        _WaveAmplitude ("Wave Height", Float) = 0.5
        _WaveFrequency ("Wave Frequency", Float) = 1
        _WaveSpeed ("Wave Speed", Float) = 1
        _MainTex ("Main Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert

        sampler2D _MainTex;
        float4 _Color;
        float _WaveAmplitude;
        float _WaveFrequency;
        float _WaveSpeed;

        struct Input
        {
            float2 uv_MainTex;
        };

        void vert(inout appdata_full v)
        {
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            float wave = sin(_WaveFrequency * worldPos.x + _WaveSpeed * _Time.y)
                       + cos(_WaveFrequency * worldPos.z + _WaveSpeed * _Time.y);
            wave *= 0.5 * _WaveAmplitude;
            v.vertex.y += wave;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 tex = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = _Color.rgb * tex.rgb;
            o.Metallic = 0;
            o.Smoothness = 0.8;
            o.Alpha = 1;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
