using System.Collections.Generic;
using UnityEngine;

public static class IngredientPlaceholderSpriteFactory
{
    private const int TextureSize = 128;
    private const float PixelsPerUnit = 64f;

    private static readonly Dictionary<int, Sprite>
        cache =
            new Dictionary<int, Sprite>();

    public static Sprite GetOrCreate(
        IngredientDefinition definition
    )
    {
        if (
            definition == null ||
            definition.placeholderShape ==
                PlaceholderIngredientShape.None
        )
        {
            return null;
        }

        int key =
            definition.GetInstanceID();

        if (
            cache.TryGetValue(
                key,
                out Sprite existing
            ) &&
            existing != null
        )
        {
            return existing;
        }

        Texture2D texture =
            new Texture2D(
                TextureSize,
                TextureSize,
                TextureFormat.RGBA32,
                false
            );

        texture.name =
            $"{definition.ingredientName}_PlaceholderTexture";

        texture.filterMode =
            FilterMode.Bilinear;

        texture.wrapMode =
            TextureWrapMode.Clamp;

        Color[] pixels =
            new Color[
                TextureSize *
                TextureSize
            ];

        Color clear =
            new Color(
                0f,
                0f,
                0f,
                0f
            );

        Color outline =
            new Color(
                0.12f,
                0.07f,
                0.03f,
                1f
            );

        Color fill =
            definition.placeholderColor;

        fill.a =
            1f;

        for (
            int y = 0;
            y < TextureSize;
            y++
        )
        {
            for (
                int x = 0;
                x < TextureSize;
                x++
            )
            {
                Vector2 point =
                    PixelToNormalized(
                        x,
                        y
                    );

                bool insideOuter =
                    IsInside(
                        definition.placeholderShape,
                        point,
                        0f
                    );

                if (!insideOuter)
                {
                    pixels[
                        y * TextureSize +
                        x
                    ] = clear;

                    continue;
                }

                bool insideInner =
                    IsInside(
                        definition.placeholderShape,
                        point,
                        0.075f
                    );

                Color color =
                    insideInner
                        ? AddPaperVariation(
                            fill,
                            x,
                            y
                        )
                        : outline;

                pixels[
                    y * TextureSize +
                    x
                ] = color;
            }
        }

        texture.SetPixels(
            pixels
        );

        texture.Apply(
            false,
            false
        );

        Sprite sprite =
            Sprite.Create(
                texture,
                new Rect(
                    0f,
                    0f,
                    TextureSize,
                    TextureSize
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                PixelsPerUnit,
                0,
                SpriteMeshType.FullRect
            );

        sprite.name =
            $"{definition.ingredientName}_PlaceholderSprite";

        cache[key] =
            sprite;

        return sprite;
    }

    private static Vector2 PixelToNormalized(
        int x,
        int y
    )
    {
        return new Vector2(
            (
                x + 0.5f
            ) /
            TextureSize *
            2f -
            1f,
            (
                y + 0.5f
            ) /
            TextureSize *
            2f -
            1f
        );
    }

    private static bool IsInside(
        PlaceholderIngredientShape shape,
        Vector2 point,
        float inset
    )
    {
        float safe =
            Mathf.Clamp(
                inset,
                0f,
                0.35f
            );

        switch (shape)
        {
            case PlaceholderIngredientShape.Round:
                return
                    point.sqrMagnitude <=
                    Mathf.Pow(
                        0.88f - safe,
                        2f
                    );

            case PlaceholderIngredientShape.Oval:
                return IsInsideEllipse(
                    point,
                    0.72f - safe,
                    0.9f - safe
                );

            case PlaceholderIngredientShape.Box:
                return
                    Mathf.Abs(point.x) <=
                        0.82f - safe &&
                    Mathf.Abs(point.y) <=
                        0.68f - safe;

            case PlaceholderIngredientShape.Triangle:
                return IsInsideTriangle(
                    point,
                    safe
                );

            case PlaceholderIngredientShape.Long:
                return IsInsideEllipse(
                    point,
                    0.9f - safe,
                    0.42f -
                        safe * 0.65f
                );

            default:
                return false;
        }
    }

    private static bool IsInsideEllipse(
        Vector2 point,
        float radiusX,
        float radiusY
    )
    {
        if (
            radiusX <= 0f ||
            radiusY <= 0f
        )
        {
            return false;
        }

        float x =
            point.x /
            radiusX;

        float y =
            point.y /
            radiusY;

        return
            x * x +
            y * y <=
            1f;
    }

    private static bool IsInsideTriangle(
        Vector2 point,
        float inset
    )
    {
        Vector2 a =
            new Vector2(
                0f,
                0.9f - inset
            );

        Vector2 b =
            new Vector2(
                -0.82f + inset,
                -0.75f + inset
            );

        Vector2 c =
            new Vector2(
                0.82f - inset,
                -0.75f + inset
            );

        float d1 =
            Sign(
                point,
                a,
                b
            );

        float d2 =
            Sign(
                point,
                b,
                c
            );

        float d3 =
            Sign(
                point,
                c,
                a
            );

        bool hasNegative =
            d1 < 0f ||
            d2 < 0f ||
            d3 < 0f;

        bool hasPositive =
            d1 > 0f ||
            d2 > 0f ||
            d3 > 0f;

        return
            !(hasNegative &&
              hasPositive);
    }

    private static float Sign(
        Vector2 p1,
        Vector2 p2,
        Vector2 p3
    )
    {
        return
            (
                p1.x -
                p3.x
            ) *
            (
                p2.y -
                p3.y
            ) -
            (
                p2.x -
                p3.x
            ) *
            (
                p1.y -
                p3.y
            );
    }

    private static Color AddPaperVariation(
        Color baseColor,
        int x,
        int y
    )
    {
        float noise =
            Mathf.Sin(
                x * 12.9898f +
                y * 78.233f
            ) *
            43758.5453f;

        noise =
            noise -
            Mathf.Floor(noise);

        float brightness =
            Mathf.Lerp(
                0.94f,
                1.04f,
                noise
            );

        return new Color(
            Mathf.Clamp01(
                baseColor.r *
                brightness
            ),
            Mathf.Clamp01(
                baseColor.g *
                brightness
            ),
            Mathf.Clamp01(
                baseColor.b *
                brightness
            ),
            1f
        );
    }
}
