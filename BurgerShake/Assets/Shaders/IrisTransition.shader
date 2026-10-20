Shader "BurgerShake/IrisTransition"
{
    Properties
    {
        [PerRendererData]
        _MainTex (
            "Sprite Texture",
            2D
        ) = "white" {}

        _IrisColor (
            "Iris Color",
            Color
        ) = (0, 0, 0, 1)

        _IrisCenter (
            "Iris Center",
            Vector
        ) = (0.5, 0.5, 0, 0)

        _IrisRadius (
            "Iris Radius",
            Float
        ) = 2

        _EdgeSoftness (
            "Edge Softness",
            Float
        ) = 0.003

        _Aspect (
            "Screen Aspect",
            Float
        ) = 1.7777778
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        ZWrite Off
        ZTest Always

        Blend SrcAlpha OneMinusSrcAlpha

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
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _IrisColor;
            float4 _IrisCenter;

            float _IrisRadius;
            float _EdgeSoftness;
            float _Aspect;

            Varyings vert(
                Attributes input
            )
            {
                Varyings output;

                output.positionHCS =
                    TransformObjectToHClip(
                        input.positionOS.xyz
                    );

                output.uv =
                    input.uv;

                output.color =
                    input.color;

                return output;
            }

            half4 frag(
                Varyings input
            ) : SV_Target
            {
                // Unity UI expects _MainTex to exist.
                // We sample its alpha so the shader
                // remains compatible with UI Images.
                half textureAlpha =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv
                    ).a;

                if (_IrisRadius <= 0.0001)
                {
                    return half4(
                        _IrisColor.rgb,
                        _IrisColor.a *
                        input.color.a *
                        textureAlpha
                    );
                }

                float2 difference =
                    input.uv -
                    _IrisCenter.xy;

                // Correct the circle so it stays
                // circular on widescreen displays.
                difference.x *=
                    _Aspect;

                float distanceFromCenter =
                    length(
                        difference
                    );

                float softness =
                    max(
                        0.0001,
                        _EdgeSoftness
                    );

                float alpha =
                    smoothstep(
                        _IrisRadius -
                            softness,
                        _IrisRadius +
                            softness,
                        distanceFromCenter
                    );

                return half4(
                    _IrisColor.rgb,
                    alpha *
                    _IrisColor.a *
                    input.color.a *
                    textureAlpha
                );
            }

            ENDHLSL
        }
    }
}