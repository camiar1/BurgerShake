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

        SpriteRenderer spriteRenderer =
            GetComponent<SpriteRenderer>();

        if (
            spriteRenderer != null &&
            definition != null &&
            definition.sprite != null
        )
        {
            spriteRenderer.sprite =
                definition.sprite;
        }
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