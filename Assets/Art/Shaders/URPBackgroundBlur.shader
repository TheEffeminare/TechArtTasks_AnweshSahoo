Shader "UI/ScreenBlurUI"
{
    Properties
    {
        _Tint       ("Tint (adds dimming)", Color) = (0,0,0,0.35)
        _Radius     ("Blur Radius (px)", Range(0,20)) = 8
        _Iterations ("Iterations", Range(1,4)) = 2
        _Alpha      ("Overall Alpha", Range(0,1)) = 1

        // standard UI stencil block
        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil    ("Stencil ID", Float) = 0
        _StencilOp  ("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask",  Float) = 255
        _ColorMask  ("Color Mask", Float) = 15
        _UseUIAlphaClip("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags{ "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }

        Stencil{ Ref[_Stencil] Comp[_StencilComp] Pass[_StencilOp] ReadMask[_StencilReadMask] WriteMask[_StencilWriteMask] }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UIScreenBlur"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target 3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // texture filled by our Renderer Feature
            TEXTURE2D_X(_UIBlurTex);
            SAMPLER(sampler_UIBlurTex);

            CBUFFER_START(UnityPerMaterial)
            float4 _Tint;
            float  _Radius;
            float  _Iterations;
            float  _Alpha;
            CBUFFER_END

            struct Attributes { float4 positionOS:POSITION; float4 color:COLOR; };
            struct Varyings  { float4 positionCS:SV_POSITION; float4 color:COLOR; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.color = v.color;
                return o;
            }

            float3 SampleBlur(float2 uv, float2 texel, int iters)
            {
                float3 acc = 0; float count = 0;
                [loop] for (int k=0;k<iters;k++)
                {
                    float s = k+1;
                    float2 t = texel*s;

                    acc += SAMPLE_TEXTURE2D_X(_UIBlurTex, sampler_UIBlurTex, uv).rgb; count++;
                    acc += SAMPLE_TEXTURE2D_X(_UIBlurTex, sampler_UIBlurTex, uv + float2( t.x, 0)).rgb; count++;
                    acc += SAMPLE_TEXTURE2D_X(_UIBlurTex, sampler_UIBlurTex, uv + float2(-t.x, 0)).rgb; count++;
                    acc += SAMPLE_TEXTURE2D_X(_UIBlurTex, sampler_UIBlurTex, uv + float2(0,  t.y)).rgb; count++;
                    acc += SAMPLE_TEXTURE2D_X(_UIBlurTex, sampler_UIBlurTex, uv + float2(0, -t.y)).rgb; count++;
                    acc += SAMPLE_TEXTURE2D_X(_UIBlurTex, sampler_UIBlurTex, uv +  t).rgb; count++;
                    acc += SAMPLE_TEXTURE2D_X(_UIBlurTex, sampler_UIBlurTex, uv + -t).rgb; count++;
                    acc += SAMPLE_TEXTURE2D_X(_UIBlurTex, sampler_UIBlurTex, uv + float2( t.x, -t.y)).rgb; count++;
                    acc += SAMPLE_TEXTURE2D_X(_UIBlurTex, sampler_UIBlurTex, uv + float2(-t.x,  t.y)).rgb; count++;
                }
                return acc / max(count, 1.0);
            }

            float4 frag(Varyings i):SV_Target
            {
                float2 uv = GetNormalizedScreenSpaceUV(i.positionCS);
                float2 texel = _Radius * float2(_ScreenParams.z, _ScreenParams.w);
                int iters = max(1, (int)round(saturate(_Iterations)*4.0));
                float3 blurred = SampleBlur(uv, texel, iters);

                float3 tinted = lerp(blurred, _Tint.rgb, _Tint.a);
                float alpha = i.color.a * _Alpha;

                #if defined(_UseUIAlphaClip)
                clip(alpha - 0.001);
                #endif

                return float4(tinted, alpha);
            }
            ENDHLSL
        }
    }
}
