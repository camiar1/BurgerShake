using System.Collections.Generic;
using UnityEngine;

public class RunProgress : MonoBehaviour
{
    public int Day { get; private set; } = 1;
    public int Coins { get; private set; }
    public IReadOnlyList<IngredientDefinition> Ingredients => ingredients;
    public IReadOnlyList<RunUpgradeDefinition> Upgrades => upgrades;

    private readonly List<IngredientDefinition> ingredients = new List<IngredientDefinition>();
    private readonly List<RunUpgradeDefinition> upgrades = new List<RunUpgradeDefinition>();

    public void BeginRun(RunDefinition definition)
    {
        Day = 1;
        Coins = definition != null ? definition.startingCoins : 0;
        ingredients.Clear();
        upgrades.Clear();

        if (definition != null)
        {
            int count = Mathf.Min(definition.startingIngredientCount, definition.startingIngredients.Count);
            for (int i = 0; i < count; i++)
            {
                AddIngredient(definition.startingIngredients[i]);
            }
        }
    }

    public void AddIngredient(IngredientDefinition ingredient)
    {
        if (ingredient != null && !ingredients.Contains(ingredient))
        {
            ingredients.Add(ingredient);
        }
    }

    public void AddUpgrade(RunUpgradeDefinition upgrade)
    {
        if (upgrade != null)
        {
            upgrades.Add(upgrade);
        }
    }

    public void AddCoins(int amount)
    {
        Coins += Mathf.Max(0, amount);
    }

    public bool TrySpendCoins(int amount)
    {
        if (amount < 0 || Coins < amount)
        {
            return false;
        }

        Coins -= amount;
        return true;
    }

    public void AdvanceDay()
    {
        Day++;
    }
}
