using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredient", menuName = "Burger Shake/Ingredient")]
public class IngredientDefinition : ScriptableObject
{
    [Header("Identity")]
    public string ingredientName = "Ingredient";

    [TextArea]
    public string description;

    [Header("Visual / Prefab")]
    public Sprite sprite;
    public GameObject prefab;

    [Header("Drafting")]
    [Min(0.01f)] public float draftWeight = 1f;

    [Header("Scoring")]
    public int basePoints = 10;
    public int pointsPerTouch;
    public float baseMult;
    public float multPerTouch;

    [Header("Tags")]
    public List<IngredientTag> tags = new List<IngredientTag>();

    public bool HasTag(IngredientTag tag)
    {
        return tags.Contains(tag);
    }
}
