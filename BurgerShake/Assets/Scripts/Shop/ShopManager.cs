using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private RunProgress progress;
    [SerializeField] private IngredientDraftManager draftManager;
    [SerializeField] private UpgradeManager upgradeManager;

    [Header("Ingredient Crates")]
    [SerializeField] private List<IngredientDefinition> allIngredients = new List<IngredientDefinition>();
    [SerializeField] private int ingredientCrateCost = 5;
    [SerializeField] private int ingredientChoicesPerCrate = 3;

    [Header("Upgrades")]
    [SerializeField] private List<RunUpgradeDefinition> availableUpgrades = new List<RunUpgradeDefinition>();

    private readonly List<IngredientDefinition> currentIngredientChoices = new List<IngredientDefinition>();

    public IReadOnlyList<IngredientDefinition> CurrentIngredientChoices => currentIngredientChoices;
    public IReadOnlyList<RunUpgradeDefinition> AvailableUpgrades => availableUpgrades;

    public bool OpenIngredientCrate()
    {
        if (progress == null || !progress.TrySpendCoins(ingredientCrateCost))
        {
            return false;
        }

        currentIngredientChoices.Clear();
        List<IngredientDefinition> candidates = new List<IngredientDefinition>();

        foreach (IngredientDefinition ingredient in allIngredients)
        {
            if (ingredient != null && !Contains(progress.Ingredients, ingredient))
            {
                candidates.Add(ingredient);
            }
        }

        int count = Mathf.Min(ingredientChoicesPerCrate, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, candidates.Count);
            currentIngredientChoices.Add(candidates[index]);
            candidates.RemoveAt(index);
        }

        return currentIngredientChoices.Count > 0;
    }

    public bool ChooseIngredient(IngredientDefinition ingredient)
    {
        if (progress == null || ingredient == null || !currentIngredientChoices.Contains(ingredient))
        {
            return false;
        }

        progress.AddIngredient(ingredient);
        draftManager?.AddIngredientToPool(ingredient);
        currentIngredientChoices.Clear();
        return true;
    }

    public bool PurchaseUpgrade(RunUpgradeDefinition upgrade)
    {
        if (progress == null || upgrade == null || !availableUpgrades.Contains(upgrade))
        {
            return false;
        }

        if (!progress.TrySpendCoins(upgrade.cost))
        {
            return false;
        }

        progress.AddUpgrade(upgrade);
        upgradeManager?.ApplyOwnedUpgrades();
        return true;
    }

    private bool Contains(IReadOnlyList<IngredientDefinition> list, IngredientDefinition value)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == value)
            {
                return true;
            }
        }

        return false;
    }
}
