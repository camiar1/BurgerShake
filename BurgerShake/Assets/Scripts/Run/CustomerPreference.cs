using UnityEngine;

public enum CustomerPreferenceType
{
    IngredientCount,
    TagCount,
    ScoreOverGoal,
    MajorTypeDiversity,
    TotalContacts
}

[CreateAssetMenu(fileName = "NewCustomerPreference", menuName = "Burger Shake/Customer Preference")]
public class CustomerPreference : ScriptableObject
{
    public string preferenceName;
    [TextArea] public string description;

    public CustomerPreferenceType type;
    public IngredientDefinition ingredient;
    public IngredientTag tag;

    [Min(1)] public int requiredCount = 1;
    [Min(0)] public int scoreOverGoal = 0;
    [Min(0)] public int bonusCoins = 1;

    public bool IsSatisfied(int goalScore, int totalScore, Ingredient[] ingredients)
    {
        switch (type)
        {
            case CustomerPreferenceType.ScoreOverGoal:
                return totalScore >= goalScore + scoreOverGoal;

            case CustomerPreferenceType.IngredientCount:
                return CountIngredient(ingredients) >= requiredCount;

            case CustomerPreferenceType.TagCount:
                return CountTag(ingredients) >= requiredCount;

            case CustomerPreferenceType.MajorTypeDiversity:
                return CountMajorTypes(ingredients) >= requiredCount;

            case CustomerPreferenceType.TotalContacts:
                return CountContacts(ingredients) >= requiredCount;

            default:
                return false;
        }
    }

    private int CountIngredient(Ingredient[] ingredients)
    {
        int count = 0;
        foreach (Ingredient instance in ingredients)
        {
            if (instance != null && instance.Definition == ingredient)
            {
                count++;
            }
        }
        return count;
    }

    private int CountTag(Ingredient[] ingredients)
    {
        int count = 0;
        foreach (Ingredient instance in ingredients)
        {
            if (instance != null && instance.Definition != null && instance.Definition.HasTag(tag))
            {
                count++;
            }
        }
        return count;
    }

    private int CountMajorTypes(Ingredient[] ingredients)
    {
        bool hasFruit = false;
        bool hasProtein = false;
        bool hasVegetable = false;

        foreach (Ingredient instance in ingredients)
        {
            if (instance == null || instance.Definition == null)
                continue;

            hasFruit |= instance.Definition.HasTag(IngredientTag.Fruit);
            hasProtein |= instance.Definition.HasTag(IngredientTag.Protein);
            hasVegetable |= instance.Definition.HasTag(IngredientTag.Vegetable);
        }

        return (hasFruit ? 1 : 0) +
               (hasProtein ? 1 : 0) +
               (hasVegetable ? 1 : 0);
    }

    private int CountContacts(Ingredient[] ingredients)
    {
        int totalTouches = 0;

        foreach (Ingredient instance in ingredients)
        {
            if (instance != null)
                totalTouches += instance.TouchingCount;
        }

        return totalTouches / 2;
    }
}
