using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StartingDraftSynergyEntry
{
    public IngredientDefinition ingredient;

    [Tooltip(
        "Ingredients that make sensible second picks " +
        "after this ingredient is chosen first."
    )]
    public List<IngredientDefinition> partners =
        new List<IngredientDefinition>();
}

[CreateAssetMenu(
    fileName = "NewRunDefinition",
    menuName = "Burger Shake/Run Definition"
)]
public class RunDefinition :
    ScriptableObject
{
    [Header("Starting Draft")]
    [Tooltip(
        "Ingredients that are eligible to appear in the " +
        "run-opening ingredient draft. Starting draft " +
        "choices are equally likely and ignore Draft Weight."
    )]
    public List<IngredientDefinition>
        startingDraftIngredients =
            new List<IngredientDefinition>();

    [Tooltip(
        "Curated partner lists used to guarantee one " +
        "compatible option in the second starting-draft round."
    )]
    public List<StartingDraftSynergyEntry>
        startingDraftSynergies =
            new List<StartingDraftSynergyEntry>();

    [Header("Customers")]
    public List<CustomerDefinition>
        customers =
            new List<CustomerDefinition>();

    [Header("Difficulty")]
    [Tooltip(
        "Multiplies each customer's base goal by day index. " +
        "X = day number, Y = goal multiplier."
    )]
    public AnimationCurve
        goalMultiplierByDay =
            AnimationCurve.Linear(
                1f,
                1f,
                10f,
                2f
            );

    [Header("Economy")]
    [Min(0)]
    public int startingCoins =
        0;

    [Header("Run Flow")]
    [Tooltip("Customer numbers after which the shop opens. Leave empty to shop after every successful customer.")]
    public List<int> shopAfterCustomers = new List<int>();

    [Tooltip("When enabled, a failed order can retry the current customer without resetting the run.")]
    public bool allowCustomerRetry = true;

    public bool ShouldOpenShopAfterCustomer(int customerNumber)
    {
        if (customers == null || customerNumber >= customers.Count)
            return false;

        if (shopAfterCustomers == null || shopAfterCustomers.Count == 0)
            return true;

        return shopAfterCustomers.Contains(customerNumber);
    }

    public IReadOnlyList<IngredientDefinition>
        GetStartingDraftPartners(
            IngredientDefinition ingredient
        )
    {
        if (
            ingredient == null ||
            startingDraftSynergies == null
        )
        {
            return null;
        }

        foreach (
            StartingDraftSynergyEntry entry
            in startingDraftSynergies
        )
        {
            if (
                entry != null &&
                entry.ingredient == ingredient
            )
            {
                return entry.partners;
            }
        }

        return null;
    }
}
