using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Run")]
    [SerializeField]
    private RunProgress progress;

    [Header("Ingredient Crates")]
    [SerializeField]
    private List<IngredientDefinition>
        allIngredients =
            new List<
                IngredientDefinition
            >();

    [SerializeField]
    private int ingredientCrateCost =
        5;

    [SerializeField]
    private int ingredientChoicesPerCrate =
        3;

    [Header("Helpers")]
    [SerializeField]
    private List<RunUpgradeDefinition>
        availableUpgrades =
            new List<
                RunUpgradeDefinition
            >();

    [SerializeField]
    private int upgradeChoicesPerShop =
        3;

    private readonly List<
        IngredientDefinition
    > currentIngredientChoices =
        new List<
            IngredientDefinition
        >();

    private readonly List<
        RunUpgradeDefinition
    > currentUpgradeChoices =
        new List<
            RunUpgradeDefinition
        >();

    public IReadOnlyList<
        IngredientDefinition
    > CurrentIngredientChoices =>
        currentIngredientChoices;

    public IReadOnlyList<
        RunUpgradeDefinition
    > CurrentUpgradeChoices =>
        currentUpgradeChoices;

    public int IngredientCrateCost =>
        ingredientCrateCost;

    public bool HasOpenIngredientCrate =>
        currentIngredientChoices.Count >
        0;

    private void Awake()
    {
        if (progress == null)
        {
            progress =
                FindFirstObjectByType<
                    RunProgress
                >();
        }
    }

    public void BeginShop()
    {
        currentIngredientChoices.Clear();

        GenerateUpgradeChoices();
    }

    public bool CanOpenIngredientCrate()
    {
        if (progress == null)
        {
            return false;
        }

        if (HasOpenIngredientCrate)
        {
            return false;
        }

        if (
            progress.Coins <
            ingredientCrateCost
        )
        {
            return false;
        }

        return
            GetIngredientCandidates()
                .Count > 0;
    }

    public bool OpenIngredientCrate()
    {
        if (progress == null)
        {
            return false;
        }

        if (HasOpenIngredientCrate)
        {
            return false;
        }

        List<IngredientDefinition>
            candidates =
                GetIngredientCandidates();

        if (candidates.Count == 0)
        {
            return false;
        }

        if (
            !progress.TrySpendCoins(
                ingredientCrateCost
            )
        )
        {
            return false;
        }

        currentIngredientChoices.Clear();

        int count =
            Mathf.Min(
                ingredientChoicesPerCrate,
                candidates.Count
            );

        for (
            int i = 0;
            i < count;
            i++
        )
        {
            int index =
                Random.Range(
                    0,
                    candidates.Count
                );

            IngredientDefinition chosen =
                candidates[index];

            currentIngredientChoices.Add(
                chosen
            );

            candidates.RemoveAt(
                index
            );
        }

        return
            currentIngredientChoices
                .Count > 0;
    }

    public bool ChooseIngredient(
        IngredientDefinition ingredient
    )
    {
        if (
            progress == null ||
            ingredient == null ||
            !currentIngredientChoices
                .Contains(
                    ingredient
                )
        )
        {
            return false;
        }

        progress.AddIngredientCopies(
            ingredient,
            1
        );

        currentIngredientChoices.Clear();

        return true;
    }

    public bool PurchaseUpgrade(
        RunUpgradeDefinition upgrade
    )
    {
        if (
            progress == null ||
            upgrade == null ||
            !currentUpgradeChoices
                .Contains(
                    upgrade
                )
        )
        {
            return false;
        }

        if (
            progress.HasUpgrade(
                upgrade
            )
        )
        {
            return false;
        }

        if (
            !progress.TrySpendCoins(
                upgrade.cost
            )
        )
        {
            return false;
        }

        progress.AddUpgrade(
            upgrade
        );

        currentUpgradeChoices.Remove(
            upgrade
        );

        return true;
    }

    private void GenerateUpgradeChoices()
    {
        currentUpgradeChoices.Clear();

        List<RunUpgradeDefinition>
            candidates =
                new List<
                    RunUpgradeDefinition
                >();

        foreach (
            RunUpgradeDefinition upgrade
            in availableUpgrades
        )
        {
            if (
                upgrade == null ||
                progress == null ||
                progress.HasUpgrade(
                    upgrade
                )
            )
            {
                continue;
            }

            candidates.Add(
                upgrade
            );
        }

        int count =
            Mathf.Min(
                upgradeChoicesPerShop,
                candidates.Count
            );

        for (
            int i = 0;
            i < count;
            i++
        )
        {
            int index =
                Random.Range(
                    0,
                    candidates.Count
                );

            RunUpgradeDefinition chosen =
                candidates[index];

            currentUpgradeChoices.Add(
                chosen
            );

            candidates.RemoveAt(
                index
            );
        }
    }

    private List<IngredientDefinition>
        GetIngredientCandidates()
    {
        List<IngredientDefinition>
            candidates =
                new List<
                    IngredientDefinition
                >();

        foreach (
            IngredientDefinition ingredient
            in allIngredients
        )
        {
            if (
                ingredient == null ||
                candidates.Contains(
                    ingredient
                )
            )
            {
                continue;
            }

            candidates.Add(
                ingredient
            );
        }

        return candidates;
    }
}