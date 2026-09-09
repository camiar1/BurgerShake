using UnityEngine;

[CreateAssetMenu(
    fileName = "NewUniqueNeighborRule",
    menuName =
        "Burger Shake/Scoring Rules/Unique Neighbors"
)]
public class UniqueNeighborScoringRule :
    IngredientScoringRule
{
    [Min(1)]
    public int minimumUniqueNeighbors = 1;

    public bool rewardPerUniqueNeighbor = true;

    public override ScoreValue Evaluate(
        Ingredient ingredient
    )
    {
        if (ingredient == null)
        {
            return default;
        }

        int uniqueCount =
            ingredient
                .CountUniqueTouchingIngredients();

        if (
            uniqueCount <
            minimumUniqueNeighbors
        )
        {
            return default;
        }

        return CreateReward(
            rewardPerUniqueNeighbor
                ? uniqueCount
                : 1f
        );
    }
}