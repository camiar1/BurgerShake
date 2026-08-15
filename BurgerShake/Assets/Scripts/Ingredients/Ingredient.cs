using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ingredient : MonoBehaviour
{
    [SerializeField] private IngredientDefinition definition;

    private readonly HashSet<Ingredient> touchingIngredients = new HashSet<Ingredient>();

    public IngredientDefinition Definition => definition;
    public IReadOnlyCollection<Ingredient> TouchingIngredients => touchingIngredients;
    public int TouchingCount => touchingIngredients.Count;

    public void Initialize(IngredientDefinition newDefinition)
    {
        definition = newDefinition;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && definition != null && definition.sprite != null)
        {
            spriteRenderer.sprite = definition.sprite;
        }
    }

    public int CalculatePoints()
    {
        if (definition == null)
        {
            return 0;
        }

        return definition.basePoints + (definition.pointsPerTouch * TouchingCount);
    }

    public float CalculateMult()
    {
        if (definition == null)
        {
            return 0f;
        }

        return definition.baseMult + (definition.multPerTouch * TouchingCount);
    }

    public int CountTouchingWithTag(IngredientTag tag)
    {
        int count = 0;

        foreach (Ingredient ingredient in touchingIngredients)
        {
            if (ingredient.Definition != null && ingredient.Definition.HasTag(tag))
            {
                count++;
            }
        }

        return count;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Ingredient other = collision.gameObject.GetComponent<Ingredient>();

        if (other != null && other != this)
        {
            touchingIngredients.Add(other);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Ingredient other = collision.gameObject.GetComponent<Ingredient>();

        if (other != null)
        {
            touchingIngredients.Remove(other);
        }
    }
}
