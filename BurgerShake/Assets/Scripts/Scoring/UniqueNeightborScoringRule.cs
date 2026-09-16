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

    [Tooltip(
        "If enabled, Amount is awarded for each unique neighboring ingredient type."
    )]
    public bool rewardPerUniqueNeighbor = true;

    [Tooltip(
        "When rewarding per unique neighbor, this many unique neighbors satisfy the condition but do not add to the reward count. " +
        "Example: Minimum 3, Ignored 2 means only the 3rd, 4th, 5th... unique neighbors score."
    )]
    [Min(0)]
    public int uniqueNeighborsIgnoredForReward = 0;

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

        if (!rewardPerUniqueNeighbor)
        {
            return CreateReward(1f);
        }

        int rewardedNeighbors =
            Mathf.Max(
                0,
                uniqueCount -
                uniqueNeighborsIgnoredForReward
            );

        return CreateReward(
            rewardedNeighbors
        );
    }
}
