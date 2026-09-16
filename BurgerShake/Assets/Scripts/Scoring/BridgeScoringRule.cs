using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewBridgeRule",
    menuName =
        "Burger Shake/Scoring Rules/Bridge"
)]
public class BridgeScoringRule :
    IngredientScoringRule
{
    [Tooltip(
        "If enabled, each separate pair bridged " +
        "by this ingredient triggers the reward."
    )]
    public bool rewardPerBridgePair;

    public override ScoreValue Evaluate(
        Ingredient ingredient
    )
    {
        if (ingredient == null)
        {
            return default;
        }

        List<Ingredient> neighbors =
            new List<Ingredient>(
                ingredient.TouchingIngredients
            );

        int bridgePairs = 0;

        for (
            int i = 0;
            i < neighbors.Count;
            i++
        )
        {
            for (
                int j = i + 1;
                j < neighbors.Count;
                j++
            )
            {
                Ingredient first =
                    neighbors[i];

                Ingredient second =
                    neighbors[j];

                if (
                    first != null &&
                    second != null &&
                    !first.IsTouching(second)
                )
                {
                    bridgePairs++;
                }
            }
        }

        if (bridgePairs == 0)
        {
            return default;
        }

        return CreateReward(
            rewardPerBridgePair
                ? bridgePairs
                : 1f
        );
    }
}