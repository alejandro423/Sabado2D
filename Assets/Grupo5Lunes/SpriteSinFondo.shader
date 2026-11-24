Shader "Grupo5Lunes/SpriteSinFondo"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Modo ("0: fondo, 1: gris, 2: cuadricula oscura", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            struct Attributes { float3 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _Modo;
            CBUFFER_END
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                // These JPEG sheets have baked backgrounds. Hide them at render time
                // while preserving the supplied images and their sprite rectangles.
                half3 srgb = LinearToSRGB(color.rgb);
                #if defined(UNITY_COLORSPACE_GAMMA)
                    srgb = color.rgb;
                #endif
                half maximum = max(srgb.r, max(srgb.g, srgb.b));
                half minimum = min(srgb.r, min(srgb.g, srgb.b));
                if (_Modo > 0.5 && _Modo < 1.5)
                {
                    if (maximum - minimum < 0.065 && abs(maximum - 0.5) < 0.09) discard;
                }
                if (_Modo > 1.5 && maximum < 0.23) discard;
                return color * input.color;
            }
            ENDHLSL
        }
    }
}
