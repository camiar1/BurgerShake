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

    [Header("Contents")]
    public IngredientTag requiredTag;

    [Min(1)]
    public int choices =
        3;

    [Header("Economy")]
    [Min(0)]
    public int cost =
        5;
}