using UnityEngine;

[CreateAssetMenu(
    fileName = "NewClusterRule",
    menuName =
        "Burger Shake/Scoring Rules/Same Ingredient Cluster"
)]
public class ClusterScoringRule :
    IngredientScoringRule
{
    [Min(1)]
    public int minimumClusterSize = 1;

    public bool rewardPerClusterMember = true;

    public override ScoreValue Evaluate(
        Ingredient ingredient
    )
    {
        if (ingredient == null)
        {
            return default;
        }

        int clusterSize =
            ingredient
                .GetSameIngredientClusterSize();

        if (
            clusterSize <
            minimumClusterSize
        )
        {
            return default;
        }

        return CreateReward(
            rewardPerClusterMember
                ? clusterSize
                : 1f
        );
    }
}