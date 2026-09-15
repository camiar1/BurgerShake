using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewTagDiversityRule",
    menuName =
        "Burger Shake/Scoring Rules/Tag Diversity"
)]
public class TagDiversityScoringRule :
    IngredientScoringRule
{
    [Header("Categories To Look For")]
    public List<IngredientTag> countedTags =
        new List<IngredientTag>();

    [Min(1)]
    public int minimumDifferentTags = 1;

    public bool rewardPerDifferentTag = true;

    public override ScoreValue Evaluate(
        Ingredient ingredient
    )
    {
        if (ingredient == null)
        {
            return default;
        }

        int differentTags = 0;

        foreach (
            IngredientTag tag
            in countedTags
        )
        {
            if (
                ingredient.CountTouchingWithTag(
                    tag
                ) > 0
            )
            {
                differentTags++;
            }
        }

        if (
            differentTags <
            minimumDifferentTags
        )
        {
            return default;
        }

        return CreateReward(
            rewardPerDifferentTag
                ? differentTags
                : 1f
        );
    }
}