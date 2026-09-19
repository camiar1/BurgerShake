using System.Collections.Generic;
using UnityEngine;

public class CenterlineHoverGuide :
    MonoBehaviour
{
    private const float DotSpacing = 0.16f;
    private const float DotSize = 0.09f;
    private const int MaxDots = 256;

    private static readonly Color GuideColor =
        new Color(
            0.05f,
            1f,
            1f,
            0.95f
        );

    private static Sprite dotSprite;

    private readonly List<SpriteRenderer>
        dots =
            new List<SpriteRenderer>();

    private CenterlineScoringRule
        activeRule;

    private Camera activeCamera;
    private Color activeColor;

    public void Show(
        CenterlineScoringRule rule,
        Camera camera,
        Color color
    )
    {
        activeRule = rule;
        activeCamera = camera;
        activeColor = GuideColor;

        RefreshDots();
    }
    public void Hide()
    {
        activeRule = null;

        foreach (
            SpriteRenderer dot
            in dots
        )
        {
            if (dot != null)
            {
                dot.enabled = false;
            }
        }
    }

    private void LateUpdate()
    {
        if (activeRule != null)
        {
            RefreshDots();
        }
    }

    private void RefreshDots()
    {
        if (
            activeRule == null ||
            activeCamera == null
        )
        {
            Hide();
            return;
        }

        float worldLength =
            GetVisibleWorldLength(
                activeCamera
            );

        Vector3 scale =
            transform.lossyScale;

        float axisScale =
            activeRule.axis ==
                CenterlineAxis.Horizontal
                ? Mathf.Abs(scale.x)
                : Mathf.Abs(scale.y);

        axisScale =
            Mathf.Max(
                0.01f,
                axisScale
            );

        float localLength =
            worldLength /
            axisScale;

        int dotCount =
            Mathf.Clamp(
                Mathf.CeilToInt(
                    worldLength /
                    DotSpacing
                ) + 1,
                3,
                MaxDots
            );

        EnsureDotCount(
            dotCount
        );
        SpriteRenderer sourceRenderer =
            GetComponentInChildren<
                SpriteRenderer
            >();

        int sortingLayerId =
            sourceRenderer != null
                ? sourceRenderer
                    .sortingLayerID
                : 0;

        int sortingOrder =
            sourceRenderer != null
                ? sourceRenderer
                    .sortingOrder + 50
                : 50;

        Vector2 localDirection =
            activeRule.axis ==
                CenterlineAxis.Horizontal
                ? Vector2.right
                : Vector2.up;

        float localStep =
            dotCount > 1
                ? localLength /
                    (dotCount - 1)
                : 0f;

        float start =
            -localLength * 0.5f;

        float inverseX =
            DotSize /
            Mathf.Max(
                0.01f,
                Mathf.Abs(scale.x)
            );

        float inverseY =
            DotSize /
            Mathf.Max(
                0.01f,
                Mathf.Abs(scale.y)
            );

        for (
            int i = 0;
            i < dots.Count;
            i++
        )
        {
            SpriteRenderer dot =
                dots[i];

            bool visible =
                i < dotCount;

            dot.enabled = visible;

            if (!visible)
            {
                continue;
            }

            float distance =
                start +
                localStep * i;

            dot.transform.localPosition =
                localDirection *
                distance;

            dot.transform.localRotation =
                Quaternion.identity;

            dot.transform.localScale =
                new Vector3(
                    inverseX,
                    inverseY,
                    1f
                );

            dot.color =
                activeColor;

            dot.sortingLayerID =
                sortingLayerId;

            dot.sortingOrder =
                sortingOrder;
        }
    }
    private void EnsureDotCount(
        int count
    )
    {
        Sprite sprite =
            GetDotSprite();

        while (dots.Count < count)
        {
            GameObject dotObject =
                new GameObject(
                    "Centerline Dot"
                );

            dotObject.transform
                .SetParent(
                    transform,
                    false
                );

            SpriteRenderer renderer =
                dotObject.AddComponent<
                    SpriteRenderer
                >();

            renderer.sprite =
                sprite;

            dots.Add(
                renderer
            );
        }
    }

    private float GetVisibleWorldLength(
        Camera camera
    )
    {
        if (
            camera != null &&
            camera.orthographic
        )
        {
            float height =
                camera
                    .orthographicSize *
                2f;

            float width =
                height *
                camera.aspect;

            // The viewport diagonal covers the
            // whole blender even when the
            // ingredient is rotated diagonally.
            return
                Mathf.Sqrt(
                    width * width +
                    height * height
                ) + 2f;
        }

        return
            Mathf.Min(
                activeRule.lineLength,
                40f
            );
    }
    private static Sprite GetDotSprite()
    {
        if (dotSprite != null)
        {
            return dotSprite;
        }

        const int size = 24;

        Texture2D texture =
            new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false
            );

        texture.name =
            "Runtime Centerline Dot";

        texture.filterMode =
            FilterMode.Bilinear;

        texture.wrapMode =
            TextureWrapMode.Clamp;

        float center =
            (size - 1) * 0.5f;

        float radius =
            size * 0.39f;

        for (
            int y = 0;
            y < size;
            y++
        )
        {
            for (
                int x = 0;
                x < size;
                x++
            )
            {
                float dx =
                    x - center;

                float dy =
                    y - center;

                float distance =
                    Mathf.Sqrt(
                        dx * dx +
                        dy * dy
                    );

                float alpha =
                    Mathf.Clamp01(
                        radius -
                        distance +
                        0.75f
                    );

                texture.SetPixel(
                    x,
                    y,
                    new Color(
                        1f,
                        1f,
                        1f,
                        alpha
                    )
                );
            }
        }

        texture.Apply();

        dotSprite =
            Sprite.Create(
                texture,
                new Rect(
                    0f,
                    0f,
                    size,
                    size
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                size
            );

        dotSprite.name =
            "Runtime Centerline Dot";

        return dotSprite;
    }
}
