using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCrossTagRule",
    menuName =
        "Burger Shake/Scoring Rules/Cross Tag"
)]
public class CrossTagScoringRule :
    IngredientScoringRule
{
    [Header("Required Neighbor Categories")]
    public IngredientTag firstTag;

    public IngredientTag secondTag;

    public override ScoreValue Evaluate(
        Ingredient ingredient
    )
    {
        if (ingredient == null)
        {
            return default;
        }

        bool hasFirst =
            ingredient.CountTouchingWithTag(
                firstTag
            ) > 0;

        bool hasSecond =
            ingredient.CountTouchingWithTag(
                secondTag
            ) > 0;

        if (
            !hasFirst ||
            !hasSecond
        )
        {
            return default;
        }

        return CreateReward(1f);
    }
}