using UnityEngine;

public enum VerticalRelationshipType
{
    TouchingAbove,
    TouchingBelow,
    TouchingAboveAndBelow,
    NothingTouchingAbove,
    NothingTouchingBelow
}

[CreateAssetMenu(
    fileName = "NewVerticalRelationshipRule",
    menuName =
        "Burger Shake/Scoring Rules/Vertical Relationship"
)]
public class VerticalRelationshipScoringRule :
    IngredientScoringRule
{
    [Header("Vertical Condition")]
    public VerticalRelationshipType relationship;

    [Min(0f)]
    public float minimumVerticalDifference =
        0.01f;

    [Header("Optional Tag Filter")]
    public bool requireTag;

    public IngredientTag touchingTag;

    [Header("Reward")]
    public bool rewardPerMatchingIngredient;

    public override ScoreValue Evaluate(
        Ingredient ingredient
    )
    {
        if (ingredient == null)
        {
            return default;
        }

        int above =
            requireTag
                ? ingredient
                    .CountTouchingAboveWithTag(
                        touchingTag,
                        minimumVerticalDifference
                    )
                : ingredient
                    .CountTouchingAbove(
                        minimumVerticalDifference
                    );

        int below =
            requireTag
                ? ingredient
                    .CountTouchingBelowWithTag(
                        touchingTag,
                        minimumVerticalDifference
                    )
                : ingredient
                    .CountTouchingBelow(
                        minimumVerticalDifference
                    );

        switch (relationship)
        {
            case VerticalRelationshipType
                .TouchingAbove:

                if (above == 0)
                {
                    return default;
                }

                return CreateReward(
                    rewardPerMatchingIngredient
                        ? above
                        : 1f
                );

            case VerticalRelationshipType
                .TouchingBelow:

                if (below == 0)
                {
                    return default;
                }

                return CreateReward(
                    rewardPerMatchingIngredient
                        ? below
                        : 1f
                );

            case VerticalRelationshipType
                .TouchingAboveAndBelow:

                if (
                    above == 0 ||
                    below == 0
                )
                {
                    return default;
                }

                return CreateReward(1f);

            case VerticalRelationshipType
                .NothingTouchingAbove:

                return above == 0
                    ? CreateReward(1f)
                    : default;

            case VerticalRelationshipType
                .NothingTouchingBelow:

                return below == 0
                    ? CreateReward(1f)
                    : default;

            default:
                return default;
        }
    }
}