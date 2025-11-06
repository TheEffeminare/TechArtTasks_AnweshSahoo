Shader "UI/ScreenSpaceBlur_URP"
{
    Properties
    {
        [PerRendererData]_MainTex ("Sprite (Mask)", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Radius ("Radius (pixels)", Range(0,12)) = 5
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off

        // UI / stencil setup so it behaves like a normal Image
        Stencil
        {
            Ref 0
            Comp Always
            Pass Keep
        }

        Pass
        {
            Name "UI_ScreenSpaceBlur_URP"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // === textures ===
            TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
            TEXTURE2D(_CameraOpaqueTexture);SAMPLER(sampler_CameraOpaqueTexture);

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _Color;
            float  _Radius;    // in pixels
            CBUFFER_END

            struct appdata
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
                float4 color    : COLOR;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float2 uvSS     : TEXCOORD1; // screen-space UV for camera tex
                float4 color    : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos   = TransformObjectToHClip(v.vertex.xyz);
                o.uv    = TRANSFORM_TEX(v.uv, _MainTex);
                o.uvSS  = ComputeScreenPos(o.pos).xy / ComputeScreenPos(o.pos).w; // 0..1
                o.color = v.color * _Color;
                return o;
            }

            // 9-tap fast blur (box-ish). Cheap & good for dimmer backgrounds.
            float4 SampleBlur(TEXTURE2D_PARAM(tex, smp), float2 uv, float2 pixelStep, float r)
            {
                // convert radius in pixels to UV delta
                float2 d = pixelStep * r;

                float4 c = 0;
                c += SAMPLE_TEXTURE2D(tex, smp, uv);
                c += SAMPLE_TEXTURE2D(tex, smp, uv + float2( d.x, 0));
                c += SAMPLE_TEXTURE2D(tex, smp, uv + float2(-d.x, 0));
                c += SAMPLE_TEXTURE2D(tex, smp, uv + float2(0,  d.y));
                c += SAMPLE_TEXTURE2D(tex, smp, uv + float2(0, -d.y));
                c += SAMPLE_TEXTURE2D(tex, smp, uv + float2( d.x,  d.y));
                c += SAMPLE_TEXTURE2D(tex, smp, uv + float2(-d.x,  d.y));
                c += SAMPLE_TEXTURE2D(tex, smp, uv + float2( d.x, -d.y));
                c += SAMPLE_TEXTURE2D(tex, smp, uv + float2(-d.x, -d.y));
                return c / 9.0;
            }

            float4 frag (v2f i) : SV_Target
            {
                // pixel size in UV (1/width, 1/height)
                float2 pixelStep = 1.0 / _ScreenParams.xy;

                // Blur the camera color
                float4 blurred = SampleBlur(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, i.uvSS, pixelStep, _Radius);

                // multiply by the UI Image’s tint & alpha (lets you darken via Image.color)
                // also respect the sprite mask in _MainTex alpha (like a normal Image)
                float alphaMask = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).a;
                blurred.rgb *= i.color.rgb;
                blurred.a   *= i.color.a * alphaMask;

                return blurred;
            }
            ENDHLSL
        }
    }
}
