using System.Collections.Generic;
using UnityEngine;

public class IngredientHighlight :
    MonoBehaviour
{
    private class HighlightRenderer
    {
        public SpriteRenderer source;
        public SpriteRenderer outline;

        public MaterialPropertyBlock
            propertyBlock;
    }

    private readonly List<HighlightRenderer>
        highlightRenderers =
            new List<HighlightRenderer>();

    private bool built;

    private static Material outlineMaterial;

    private static readonly int
        OutlineColorProperty =
            Shader.PropertyToID(
                "_OutlineColor"
            );

    private static readonly int
        OutlineWidthProperty =
            Shader.PropertyToID(
                "_OutlineWidthPixels"
            );

    public void Show(
        Color color,
        float width
    )
    {
        if (!built)
        {
            BuildHighlight();
        }

        foreach (
            HighlightRenderer highlight
            in highlightRenderers
        )
        {
            if (
                highlight.source == null ||
                highlight.outline == null
            )
            {
                continue;
            }

            UpdateRendererSettings(
                highlight
            );

            float widthPixels =
                ConvertWorldWidthToPixels(
                    highlight.source,
                    width
                );

            highlight.outline
                .GetPropertyBlock(
                    highlight.propertyBlock
                );

            highlight.propertyBlock
                .SetColor(
                    OutlineColorProperty,
                    color
                );

            highlight.propertyBlock
                .SetFloat(
                    OutlineWidthProperty,
                    widthPixels
                );

            highlight.outline
                .SetPropertyBlock(
                    highlight.propertyBlock
                );

            highlight.outline.enabled =
                true;
        }
    }

    public void Hide()
    {
        foreach (
            HighlightRenderer highlight
            in highlightRenderers
        )
        {
            if (
                highlight.outline != null
            )
            {
                highlight.outline.enabled =
                    false;
            }
        }
    }

    private void BuildHighlight()
    {
        built = true;

        SpriteRenderer[] sources =
            GetComponentsInChildren<
                SpriteRenderer
            >(
                true
            );

        foreach (
            SpriteRenderer source
            in sources
        )
        {
            if (
                source == null ||
                source.sprite == null
            )
            {
                continue;
            }

            if (
                source.gameObject.name ==
                "IngredientHighlightOverlay"
            )
            {
                continue;
            }

            CreateHighlightRenderer(
                source
            );
        }
    }

    private void CreateHighlightRenderer(
        SpriteRenderer source
    )
    {
        GameObject outlineObject =
            new GameObject(
                "IngredientHighlightOverlay"
            );

        outlineObject.transform.SetParent(
            source.transform,
            false
        );

        outlineObject.transform.localPosition =
            Vector3.zero;

        outlineObject.transform.localRotation =
            Quaternion.identity;

        outlineObject.transform.localScale =
            Vector3.one;

        SpriteRenderer outline =
            outlineObject.AddComponent<
                SpriteRenderer
            >();

        outline.material =
            GetOutlineMaterial();

        outline.enabled =
            false;

        HighlightRenderer highlight =
            new HighlightRenderer
            {
                source =
                    source,

                outline =
                    outline,

                propertyBlock =
                    new MaterialPropertyBlock()
            };

        UpdateRendererSettings(
            highlight
        );

        highlightRenderers.Add(
            highlight
        );
    }

    private void UpdateRendererSettings(
        HighlightRenderer highlight
    )
    {
        SpriteRenderer source =
            highlight.source;

        SpriteRenderer outline =
            highlight.outline;

        outline.sprite =
            source.sprite;

        outline.flipX =
            source.flipX;

        outline.flipY =
            source.flipY;

        outline.drawMode =
            source.drawMode;

        outline.size =
            source.size;

        outline.spriteSortPoint =
            source.spriteSortPoint;

        outline.sortingLayerID =
            source.sortingLayerID;

        // Draw highlight on top of the
        // ingredient artwork.
        outline.sortingOrder =
            source.sortingOrder +
            20;

        outline.maskInteraction =
            source.maskInteraction;

        outline.color =
            Color.white;
    }

    private float ConvertWorldWidthToPixels(
        SpriteRenderer source,
        float worldWidth
    )
    {
        if (
            source == null ||
            source.sprite == null
        )
        {
            return 1f;
        }

        Vector3 scale =
            source.transform.lossyScale;

        float averageScale =
            (
                Mathf.Abs(scale.x) +
                Mathf.Abs(scale.y)
            ) *
            0.5f;

        if (averageScale <= 0.0001f)
        {
            averageScale =
                1f;
        }

        float pixels =
            worldWidth *
            source.sprite.pixelsPerUnit /
            averageScale;

        return Mathf.Clamp(
            pixels,
            1f,
            12f
        );
    }

    private static Material
        GetOutlineMaterial()
    {
        if (outlineMaterial != null)
        {
            return outlineMaterial;
        }

        Shader shader =
            Shader.Find(
                "BurgerShake/IngredientOutline"
            );

        if (shader == null)
        {
            Debug.LogError(
                "Could not find shader: " +
                "BurgerShake/IngredientOutline"
            );

            return null;
        }

        outlineMaterial =
            new Material(
                shader
            );

        outlineMaterial.name =
            "Runtime Ingredient Outline";

        return outlineMaterial;
    }
}