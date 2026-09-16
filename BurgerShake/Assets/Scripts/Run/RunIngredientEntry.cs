using System;
using UnityEngine;

[Serializable]
public class RunIngredientEntry
{
    [SerializeField]
    private IngredientDefinition ingredient;

    [SerializeField]
    [Min(1)]
    private int copies = 1;

    public IngredientDefinition Ingredient =>
        ingredient;

    public int Copies =>
        copies;

    public RunIngredientEntry(
        IngredientDefinition ingredient,
        int copies
    )
    {
        this.ingredient =
            ingredient;

        this.copies =
            Mathf.Max(
                1,
                copies
            );
    }

    public void AddCopies(
        int amount
    )
    {
        copies =
            Mathf.Max(
                1,
                copies + amount
            );
    }
}