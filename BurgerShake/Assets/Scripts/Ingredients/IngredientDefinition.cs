using System.Collections.Generic;
using UnityEngine;

public enum PlaceholderIngredientShape
{
    None,
    Round,
    Oval,
    Box,
    Triangle,
    Long
}

[CreateAssetMenu(
    fileName = "NewIngredient",
    menuName = "Burger Shake/Ingredient"
)]
public class IngredientDefinition :
    ScriptableObject
{
    [Header("Identity")]
    public string ingredientName =
        "Ingredient";

    [TextArea]
    public string description;

    [Header("Visual / Prefab")]
    public Sprite sprite;

    public GameObject prefab;

    [Header("Temporary Placeholder Visual")]
    [Tooltip(
        "Used only while this ingredient has no final sprite. " +
        "Set to None once final art is assigned."
    )]
    public PlaceholderIngredientShape placeholderShape =
        PlaceholderIngredientShape.None;

    public Color placeholderColor =
        Color.white;

    [Tooltip(
        "Approximate physical width/height for the temporary ingredient."
    )]
    public Vector2 placeholderColliderSize =
        new Vector2(2f, 2f);

    [Header("Drafting")]
    [Min(0.01f)]
    public float draftWeight =
        1f;

    [Min(1)]
    [Tooltip(
        "How many times this ingredient can appear before the ingredient bag cycles."
    )]
    public int copiesPerCycle =
        3;

    [Header("Scoring Rules")]
    [Tooltip(
        "Combine reusable scoring-rule assets to define this ingredient's behavior."
    )]
    public List<IngredientScoringRule>
        scoringRules =
            new List<
                IngredientScoringRule
            >();

    [Header("Tags")]
    public List<IngredientTag> tags =
        new List<IngredientTag>();

    private void OnEnable()
    {
        EnsureDisplaySprite();
    }

    public Sprite GetDisplaySprite()
    {
        EnsureDisplaySprite();
        return sprite;
    }

    public bool UsesPlaceholderVisual =>
        placeholderShape !=
        PlaceholderIngredientShape.None;

    public bool HasTag(
        IngredientTag tag
    )
    {
        return tags.Contains(
            tag
        );
    }

    private void EnsureDisplaySprite()
    {
        if (
            sprite != null ||
            placeholderShape ==
                PlaceholderIngredientShape.None
        )
        {
            return;
        }

        sprite =
            IngredientPlaceholderSpriteFactory
                .GetOrCreate(this);
    }
}
