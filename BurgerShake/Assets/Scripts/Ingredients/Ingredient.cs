using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ingredient : MonoBehaviour
{
    [SerializeField]
    private IngredientDefinition definition;

    private readonly HashSet<Ingredient>
        touchingIngredients =
            new HashSet<Ingredient>();

    public IngredientDefinition Definition =>
        definition;

    public IReadOnlyCollection<Ingredient>
        TouchingIngredients =>
            touchingIngredients;

    public int TouchingCount =>
        touchingIngredients.Count;

    public void Initialize(
        IngredientDefinition newDefinition
    )
    {
        definition =
            newDefinition;

        if (definition == null)
        {
            return;
        }

        SpriteRenderer spriteRenderer =
            GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            Sprite displaySprite =
                definition.GetDisplaySprite();

            if (displaySprite != null)
            {
                spriteRenderer.sprite =
                    displaySprite;
            }
        }

        ConfigurePlaceholderCollider();
    }

    private void ConfigurePlaceholderCollider()
    {
        if (
            definition == null ||
            !definition.UsesPlaceholderVisual
        )
        {
            return;
        }

        Vector2 size =
            definition.placeholderColliderSize;

        size.x =
            Mathf.Max(
                0.25f,
                size.x
            );

        size.y =
            Mathf.Max(
                0.25f,
                size.y
            );

        BoxCollider2D box =
            GetComponent<BoxCollider2D>();

        CircleCollider2D circle =
            GetComponent<CircleCollider2D>();

        CapsuleCollider2D capsule =
            GetComponent<CapsuleCollider2D>();

        PolygonCollider2D polygon =
            GetComponent<PolygonCollider2D>();

        SetColliderEnabled(box, false);
        SetColliderEnabled(circle, false);
        SetColliderEnabled(capsule, false);
        SetColliderEnabled(polygon, false);

        switch (definition.placeholderShape)
        {
            case PlaceholderIngredientShape.Round:
                if (circle == null)
                    circle = gameObject.AddComponent<CircleCollider2D>();

                circle.radius =
                    Mathf.Min(size.x, size.y) * 0.5f;

                circle.enabled = true;
                break;

            case PlaceholderIngredientShape.Oval:
            case PlaceholderIngredientShape.Long:
                if (capsule == null)
                    capsule = gameObject.AddComponent<CapsuleCollider2D>();

                capsule.size = size;
                capsule.direction =
                    size.x >= size.y
                        ? CapsuleDirection2D.Horizontal
                        : CapsuleDirection2D.Vertical;

                capsule.enabled = true;
                break;

            case PlaceholderIngredientShape.Triangle:
                if (polygon == null)
                    polygon = gameObject.AddComponent<PolygonCollider2D>();

                polygon.pathCount = 1;
                polygon.SetPath(
                    0,
                    new Vector2[]
                    {
                        new Vector2(0f, size.y * 0.5f),
                        new Vector2(-size.x * 0.5f, -size.y * 0.5f),
                        new Vector2(size.x * 0.5f, -size.y * 0.5f)
                    }
                );

                polygon.enabled = true;
                break;

            case PlaceholderIngredientShape.Box:
            default:
                if (box == null)
                    box = gameObject.AddComponent<BoxCollider2D>();

                box.size = size;
                box.enabled = true;
                break;
        }
    }

    private void SetColliderEnabled(
        Collider2D collider,
        bool enabled
    )
    {
        if (collider != null)
            collider.enabled = enabled;
    }

    public ScoreValue EvaluateScore()
    {
        ScoreValue total =
            default;

        if (definition == null)
        {
            return total;
        }

        foreach (
            IngredientScoringRule rule
            in definition.scoringRules
        )
        {
            if (rule != null)
            {
                total +=
                    rule.Evaluate(this);
            }
        }

        return total;
    }

    public bool IsTouching(
        Ingredient other
    )
    {
        return
            other != null &&
            touchingIngredients.Contains(
                other
            );
    }

    public int CountTouchingWithTag(
        IngredientTag tag
    )
    {
        int count = 0;

        foreach (
            Ingredient ingredient
            in touchingIngredients
        )
        {
            if (
                ingredient.Definition != null &&
                ingredient.Definition.HasTag(
                    tag
                )
            )
            {
                count++;
            }
        }

        return count;
    }

    public int CountTouchingIngredient(
        IngredientDefinition requiredIngredient
    )
    {
        if (requiredIngredient == null)
        {
            return 0;
        }

        int count = 0;

        foreach (
            Ingredient ingredient
            in touchingIngredients
        )
        {
            if (
                ingredient.Definition ==
                requiredIngredient
            )
            {
                count++;
            }
        }

        return count;
    }

    public int CountUniqueTouchingIngredients()
    {
        HashSet<IngredientDefinition>
            uniqueIngredients =
                new HashSet<IngredientDefinition>();

        foreach (
            Ingredient ingredient
            in touchingIngredients
        )
        {
            if (ingredient.Definition != null)
            {
                uniqueIngredients.Add(
                    ingredient.Definition
                );
            }
        }

        return uniqueIngredients.Count;
    }

    public int CountTouchingAbove(
        float minimumDifference = 0.01f
    )
    {
        int count = 0;

        foreach (
            Ingredient ingredient
            in touchingIngredients
        )
        {
            if (
                ingredient.transform.position.y >
                transform.position.y +
                minimumDifference
            )
            {
                count++;
            }
        }

        return count;
    }

    public int CountTouchingBelow(
        float minimumDifference = 0.01f
    )
    {
        int count = 0;

        foreach (
            Ingredient ingredient
            in touchingIngredients
        )
        {
            if (
                ingredient.transform.position.y <
                transform.position.y -
                minimumDifference
            )
            {
                count++;
            }
        }

        return count;
    }

    public int CountTouchingAboveWithTag(
        IngredientTag tag,
        float minimumDifference = 0.01f
    )
    {
        int count = 0;

        foreach (
            Ingredient ingredient
            in touchingIngredients
        )
        {
            if (
                ingredient.transform.position.y >
                transform.position.y +
                minimumDifference &&
                ingredient.Definition != null &&
                ingredient.Definition.HasTag(
                    tag
                )
            )
            {
                count++;
            }
        }

        return count;
    }

    public int CountTouchingBelowWithTag(
        IngredientTag tag,
        float minimumDifference = 0.01f
    )
    {
        int count = 0;

        foreach (
            Ingredient ingredient
            in touchingIngredients
        )
        {
            if (
                ingredient.transform.position.y <
                transform.position.y -
                minimumDifference &&
                ingredient.Definition != null &&
                ingredient.Definition.HasTag(
                    tag
                )
            )
            {
                count++;
            }
        }

        return count;
    }

    public int GetSameIngredientClusterSize()
    {
        if (definition == null)
        {
            return 0;
        }

        HashSet<Ingredient> visited =
            new HashSet<Ingredient>();

        Stack<Ingredient> stack =
            new Stack<Ingredient>();

        visited.Add(this);
        stack.Push(this);

        while (stack.Count > 0)
        {
            Ingredient current =
                stack.Pop();

            foreach (
                Ingredient neighbor
                in current.TouchingIngredients
            )
            {
                if (
                    neighbor == null ||
                    neighbor.Definition !=
                    definition ||
                    visited.Contains(neighbor)
                )
                {
                    continue;
                }

                visited.Add(
                    neighbor
                );

                stack.Push(
                    neighbor
                );
            }
        }

        return visited.Count;
    }

    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        Ingredient other =
            collision.gameObject
                .GetComponent<Ingredient>();

        if (
            other != null &&
            other != this
        )
        {
            touchingIngredients.Add(
                other
            );
        }
    }

    private void OnCollisionExit2D(
        Collision2D collision
    )
    {
        Ingredient other =
            collision.gameObject
                .GetComponent<Ingredient>();

        if (other != null)
        {
            touchingIngredients.Remove(
                other
            );
        }
    }
}
