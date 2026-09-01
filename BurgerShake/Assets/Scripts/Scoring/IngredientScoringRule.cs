using UnityEngine;

public enum ScoringTarget
{
    Self,
    TouchingAny,
    TouchingTag,
    TouchingIngredient
}

public enum ScoringReward
{
    Points,
    Mult
}

[CreateAssetMenu(
    fileName = "NewScoringRule",
    menuName = "Burger Shake/Scoring Rule"
)]
public class IngredientScoringRule : ScriptableObject
{
    [TextArea]
    public string description;

    [Header("Condition")]
    public ScoringTarget target =
        ScoringTarget.Self;

    public IngredientTag requiredTag;

    public IngredientDefinition requiredIngredient;

    [Header("Reward")]
    public ScoringReward reward =
        ScoringReward.Points;

    public float amount = 1f;

    public virtual ScoreValue Evaluate(
        Ingredient ingredient
    )
    {
        if (ingredient == null)
        {
            return default;
        }

        float triggerCount =
            GetTriggerCount(ingredient);

        return CreateReward(
            triggerCount
        );
    }

    protected virtual float GetTriggerCount(
        Ingredient ingredient
    )
    {
        switch (target)
        {
            case ScoringTarget.Self:
                return 1f;

            case ScoringTarget.TouchingAny:
                return ingredient.TouchingCount;

            case ScoringTarget.TouchingTag:
                return ingredient
                    .CountTouchingWithTag(
                        requiredTag
                    );

            case ScoringTarget.TouchingIngredient:
                return ingredient
                    .CountTouchingIngredient(
                        requiredIngredient
                    );

            default:
                return 0f;
        }
    }

    protected ScoreValue CreateReward(
        float triggerCount
    )
    {
        float total =
            amount * triggerCount;

        if (
            reward ==
            ScoringReward.Points
        )
        {
            return new ScoreValue(
                Mathf.RoundToInt(total),
                0f
            );
        }

        return new ScoreValue(
            0,
            total
        );
    }
}