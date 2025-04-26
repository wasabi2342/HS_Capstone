Shader "Ageia/ImageEffect/FullScreenGlitch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlitchAmount("GlitchAmount", Range(0, 1)) = 1
        [NoScaleOffset]_GlitchTex("GlitchTex", 2D) = "white" {}
        _GlitchColor1("GlitchColor1", Color) = (1, 1, 1, 1)
        _GlitchColor2("GlitchColor2", Color) = (1, 1, 1, 1)
        _GlitchColor3("GlitchColor3", Color) = (1, 1, 1, 1)
        _GlitchCutAmountX("GlitchCutAmountX", Range(0.1, 10)) = 1
        _GlitchCutAmountY("GlitchCutAmountY", Range(0.1, 10)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            TEXTURE2D(_GlitchTex);
            SAMPLER(sampler_GlitchTex);

            float _GlitchAmount;
            float3 _GlitchColor1;
            float3 _GlitchColor2;
            float3 _GlitchColor3;
            float _GlitchCutAmountX;
            float _GlitchCutAmountY;

            Varyings vert (Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                float2 screenUV = input.uv;
                
                float glitchUV = SAMPLE_TEXTURE2D(_GlitchTex, sampler_GlitchTex, 
                    float2(input.uv.x * _GlitchCutAmountX + (_Time.y * 100), 
                           input.uv.y * _GlitchCutAmountY + sin(_Time.y * 100))).r;
                
                float UV = glitchUV * _GlitchAmount;
                float glitchAmountFinal = saturate(_GlitchAmount * 10);

                // _MainTex에는 이미 UI를 포함한 전체 화면이 저장되어 있음
                float3 r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(screenUV.x + UV, screenUV.y)).r 
                           * lerp(float3(1, 0, 0), _GlitchColor1, glitchAmountFinal);
                float3 g = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(screenUV.x - UV, screenUV.y)).g 
                           * lerp(float3(0, 1, 0), _GlitchColor2, glitchAmountFinal);
                float3 b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(screenUV.x, screenUV.y + UV)).b 
                           * lerp(float3(0, 0, 1), _GlitchColor3, glitchAmountFinal);

                float3 glitchFinal = r + g + b;
                
                return half4(glitchFinal, 1);
            }
            ENDHLSL
        }
    }
}