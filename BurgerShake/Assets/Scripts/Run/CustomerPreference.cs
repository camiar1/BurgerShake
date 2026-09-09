using UnityEngine;

public enum CustomerPreferenceType
{
    IngredientCount,
    TagCount,
    ScoreOverGoal
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
}
