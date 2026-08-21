using System.Collections.Generic;
using UnityEngine;

public class RunProgress : MonoBehaviour
{
    public int Day { get; private set; } = 1;
    public int Coins { get; private set; }
    public IReadOnlyList<IngredientDefinition> Ingredients => ingredients;

    private readonly List<IngredientDefinition> ingredients = new List<IngredientDefinition>();

    public void BeginRun(RunDefinition definition)
    {
        Day = 1;
        Coins = definition != null ? definition.startingCoins : 0;
        ingredients.Clear();

        if (definition != null)
        {
            foreach (IngredientDefinition ingredient in definition.startingIngredients)
            {
                AddIngredient(ingredient);
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
