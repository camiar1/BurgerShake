Shader "BurgerShake/IngredientOutline"
{
    Properties
    {
        [PerRendererData] _MainTex (
            "Sprite Texture",
            2D
        ) = "white" {}

        _OutlineColor (
            "Outline Color",
            Color
        ) = (1, 1, 0, 1)

        _OutlineWidthPixels (
            "Outline Width",
            Float
        ) = 3

        _AlphaThreshold (
            "Alpha Threshold",
            Range(0, 1)
        ) = 0.05
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off

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
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_TexelSize;

            float4 _OutlineColor;

            float _OutlineWidthPixels;
            float _AlphaThreshold;

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

                return output;
            }

            half GetAlpha(
                float2 uv
            )
            {
                return SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    uv
                ).a;
            }

            half4 frag(
                Varyings input
            ) : SV_Target
            {
                half centerAlpha =
                    GetAlpha(
                        input.uv
                    );

                if (
                    centerAlpha <=
                    _AlphaThreshold
                )
                {
                    return half4(
                        0,
                        0,
                        0,
                        0
                    );
                }

                float2 texel =
                    _MainTex_TexelSize.xy *
                    _OutlineWidthPixels;

                half minimumAlpha =
                    1.0h;

                minimumAlpha =
                    min(
                        minimumAlpha,
                        GetAlpha(
                            input.uv +
                            float2(
                                texel.x,
                                0
                            )
                        )
                    );

                minimumAlpha =
                    min(
                        minimumAlpha,
                        GetAlpha(
                            input.uv +
                            float2(
                                -texel.x,
                                0
                            )
                        )
                    );

                minimumAlpha =
                    min(
                        minimumAlpha,
                        GetAlpha(
                            input.uv +
                            float2(
                                0,
                                texel.y
                            )
                        )
                    );

                minimumAlpha =
                    min(
                        minimumAlpha,
                        GetAlpha(
                            input.uv +
                            float2(
                                0,
                                -texel.y
                            )
                        )
                    );

                float diagonal =
                    0.70710678;

                minimumAlpha =
                    min(
                        minimumAlpha,
                        GetAlpha(
                            input.uv +
                            float2(
                                texel.x,
                                texel.y
                            ) *
                            diagonal
                        )
                    );

                minimumAlpha =
                    min(
                        minimumAlpha,
                        GetAlpha(
                            input.uv +
                            float2(
                                -texel.x,
                                texel.y
                            ) *
                            diagonal
                        )
                    );

                minimumAlpha =
                    min(
                        minimumAlpha,
                        GetAlpha(
                            input.uv +
                            float2(
                                texel.x,
                                -texel.y
                            ) *
                            diagonal
                        )
                    );

                minimumAlpha =
                    min(
                        minimumAlpha,
                        GetAlpha(
                            input.uv +
                            float2(
                                -texel.x,
                                -texel.y
                            ) *
                            diagonal
                        )
                    );

                half edge =
                    minimumAlpha <=
                    _AlphaThreshold
                        ? 1.0h
                        : 0.0h;

                return half4(
                    _OutlineColor.rgb,
                    _OutlineColor.a *
                    edge *
                    centerAlpha
                );
            }

            ENDHLSL
        }
    }
}