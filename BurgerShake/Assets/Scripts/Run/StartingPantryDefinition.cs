using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StartingPantryIngredient
{
    public IngredientDefinition ingredient;

    [Min(1)]
    public int copies = 3;
}

[CreateAssetMenu(
    fileName = "NewStartingPantry",
    menuName = "Burger Shake/Starting Pantry"
)]
public class StartingPantryDefinition :
    ScriptableObject
{
    [Header("Identity")]
    public string pantryName =
        "Starting Pantry";

    [TextArea]
    public string description;

    [Header("Starting Ingredients")]
    public List<StartingPantryIngredient>
        ingredients =
            new List<StartingPantryIngredient>();
}