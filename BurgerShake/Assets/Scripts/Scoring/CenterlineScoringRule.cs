using System.Collections.Generic;
using UnityEngine;

public enum CenterlineAxis
{
    Horizontal,
    Vertical
}

[CreateAssetMenu(
    fileName = "NewCenterlineRule",
    menuName = "Burger Shake/Scoring Rules/Centerline"
)]
public class CenterlineScoringRule :
    IngredientScoringRule
{
    [Header("Centerline")]
    public CenterlineAxis axis =
        CenterlineAxis.Horizontal;

    [Range(0.25f, 1.25f)]
    public float lengthScale = 0.9f;

    [Range(0.05f, 0.5f)]
    [Tooltip(
        "The invisible line is implemented as a forgiving thin band."
    )]
    public float widthFraction = 0.18f;

    public override ScoreValue Evaluate(
        Ingredient ingredient
    )
    {
        return CreateReward(
            CountCrossings(ingredient)
        );
    }

    public int CountCrossings(
        Ingredient ingredient
    )
    {
        if (ingredient == null)
        {
            return 0;
        }

        GetLocalBounds(
            ingredient,
            out Vector2 localCenter,
            out Vector2 localSize
        );

        Transform t =
            ingredient.transform;

        Vector3 scale =
            t.lossyScale;

        float sx =
            Mathf.Abs(scale.x);

        float sy =
            Mathf.Abs(scale.y);

        Vector2 worldSize;

        if (axis == CenterlineAxis.Horizontal)
        {
            worldSize =
                new Vector2(
                    Mathf.Max(
                        0.05f,
                        localSize.x *
                        sx *
                        lengthScale
                    ),
                    Mathf.Max(
                        0.05f,
                        localSize.y *
                        sy *
                        widthFraction
                    )
                );
        }
        else
        {
            worldSize =
                new Vector2(
                    Mathf.Max(
                        0.05f,
                        localSize.x *
                        sx *
                        widthFraction
                    ),
                    Mathf.Max(
                        0.05f,
                        localSize.y *
                        sy *
                        lengthScale
                    )
                );
        }

        Vector2 worldCenter =
            t.TransformPoint(
                localCenter
            );

        Collider2D[] hits =
            Physics2D.OverlapBoxAll(
                worldCenter,
                worldSize,
                t.eulerAngles.z
            );

        HashSet<Ingredient> unique =
            new HashSet<Ingredient>();

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
            {
                continue;
            }

            Ingredient other =
                hit.GetComponentInParent<
                    Ingredient
                >();

            if (
                other != null &&
                other != ingredient
            )
            {
                unique.Add(other);
            }
        }

        return unique.Count;
    }
    private void GetLocalBounds(
        Ingredient ingredient,
        out Vector2 center,
        out Vector2 size
    )
    {
        center = Vector2.zero;
        size = Vector2.one;

        Collider2D collider =
            ingredient.GetComponent<
                Collider2D
            >();

        if (collider is BoxCollider2D box)
        {
            center = box.offset;
            size = box.size;
            return;
        }

        if (
            collider is
            CapsuleCollider2D capsule
        )
        {
            center = capsule.offset;
            size = capsule.size;
            return;
        }

        if (
            collider is
            CircleCollider2D circle
        )
        {
            center = circle.offset;
            float diameter =
                circle.radius * 2f;
            size =
                new Vector2(
                    diameter,
                    diameter
                );
            return;
        }

        if (
            collider is
            PolygonCollider2D polygon &&
            polygon.points.Length > 0
        )
        {
            Vector2 min =
                polygon.points[0];
            Vector2 max =
                polygon.points[0];

            foreach (
                Vector2 point
                in polygon.points
            )
            {
                min =
                    Vector2.Min(
                        min,
                        point
                    );
                max =
                    Vector2.Max(
                        max,
                        point
                    );
            }

            center =
                polygon.offset +
                (min + max) * 0.5f;

            size =
                max - min;

            return;
        }
        SpriteRenderer renderer =
            ingredient.GetComponent<
                SpriteRenderer
            >();

        if (
            renderer != null &&
            renderer.sprite != null
        )
        {
            Bounds bounds =
                renderer.sprite.bounds;

            center =
                bounds.center;

            size =
                bounds.size;
        }
    }
}
