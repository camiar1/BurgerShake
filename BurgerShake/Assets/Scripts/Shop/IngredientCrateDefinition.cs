using UnityEngine;

[CreateAssetMenu(
    fileName = "NewIngredientCrate",
    menuName = "Burger Shake/Ingredient Crate"
)]
public class IngredientCrateDefinition :
    ScriptableObject
{
    [Header("Identity")]
    public string crateName =
        "Ingredient Crate";

    [TextArea]
    public string description;

    public Sprite icon;

    [Header("Contents")]
    public IngredientTag requiredTag;

    [Min(1)]
    public int choices =
        3;

    [Header("Copies Granted")]
    [Tooltip(
        "Copies granted when choosing an ingredient " +
        "the player does not already own."
    )]
    [Min(1)]
    public int newIngredientCopies =
        2;

    [Tooltip(
        "Copies granted when choosing an ingredient " +
        "already in the pantry."
    )]
    [Min(1)]
    public int existingIngredientCopies =
        1;

    [Header("Economy")]
    [Min(0)]
    public int cost =
        5;
}