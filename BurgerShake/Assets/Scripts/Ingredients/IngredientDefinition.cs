using System.Collections.Generic;
using UnityEngine;

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

    public bool HasTag(
        IngredientTag tag
    )
    {
        return tags.Contains(
            tag
        );
    }
}