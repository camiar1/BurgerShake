using System.Collections.Generic;
using UnityEngine;

public class RunProgress : MonoBehaviour
{
    public int Day
    {
        get;
        private set;
    } = 1;

    public int Coins
    {
        get;
        private set;
    }

    private readonly List<RunIngredientEntry>
        pantry =
            new List<RunIngredientEntry>();

    private readonly List<RunUpgradeDefinition>
        upgrades =
            new List<RunUpgradeDefinition>();

    public IReadOnlyList<RunIngredientEntry>
        Pantry =>
            pantry;

    public IReadOnlyList<RunUpgradeDefinition>
        Upgrades =>
            upgrades;

    public void BeginRun(
        RunDefinition definition,
        StartingPantryDefinition
            startingPantry
    )
    {
        Day =
            1;

        Coins =
            definition != null
                ? definition.startingCoins
                : 0;

        pantry.Clear();
        upgrades.Clear();

        if (
            startingPantry == null ||
            startingPantry.ingredients ==
                null
        )
        {
            return;
        }

        foreach (
            StartingPantryIngredient entry
            in startingPantry.ingredients
        )
        {
            if (
                entry == null ||
                entry.ingredient == null
            )
            {
                continue;
            }

            AddIngredientCopies(
                entry.ingredient,
                Mathf.Max(
                    1,
                    entry.copies
                )
            );
        }
    }

    public void AddIngredientCopies(
        IngredientDefinition ingredient,
        int amount = 1
    )
    {
        if (
            ingredient == null ||
            amount <= 0
        )
        {
            return;
        }

        RunIngredientEntry existing =
            GetPantryEntry(
                ingredient
            );

        if (existing != null)
        {
            existing.AddCopies(
                amount
            );

            return;
        }

        pantry.Add(
            new RunIngredientEntry(
                ingredient,
                amount
            )
        );
    }

    public int GetIngredientCopies(
        IngredientDefinition ingredient
    )
    {
        RunIngredientEntry entry =
            GetPantryEntry(
                ingredient
            );

        return entry != null
            ? entry.Copies
            : 0;
    }

    public bool HasIngredient(
        IngredientDefinition ingredient
    )
    {
        return
            GetPantryEntry(
                ingredient
            ) != null;
    }

    public RunIngredientEntry
        GetPantryEntry(
            IngredientDefinition ingredient
        )
    {
        if (ingredient == null)
        {
            return null;
        }

        foreach (
            RunIngredientEntry entry
            in pantry
        )
        {
            if (
                entry != null &&
                entry.Ingredient ==
                    ingredient
            )
            {
                return entry;
            }
        }

        return null;
    }

    public List<IngredientDefinition>
        GetIngredientDefinitions()
    {
        List<IngredientDefinition>
            definitions =
                new List<
                    IngredientDefinition
                >();

        foreach (
            RunIngredientEntry entry
            in pantry
        )
        {
            if (
                entry != null &&
                entry.Ingredient != null
            )
            {
                definitions.Add(
                    entry.Ingredient
                );
            }
        }

        return definitions;
    }

    public void AddUpgrade(
        RunUpgradeDefinition upgrade
    )
    {
        if (
            upgrade != null &&
            !upgrades.Contains(
                upgrade
            )
        )
        {
            upgrades.Add(
                upgrade
            );
        }
    }

    public bool HasUpgrade(
        RunUpgradeDefinition upgrade
    )
    {
        return
            upgrade != null &&
            upgrades.Contains(
                upgrade
            );
    }

    public void AddCoins(
        int amount
    )
    {
        Coins +=
            Mathf.Max(
                0,
                amount
            );
    }

    public bool TrySpendCoins(
        int amount
    )
    {
        if (
            amount < 0 ||
            Coins < amount
        )
        {
            return false;
        }

        Coins -=
            amount;

        return true;
    }

    public void AdvanceDay()
    {
        Day++;
    }
}