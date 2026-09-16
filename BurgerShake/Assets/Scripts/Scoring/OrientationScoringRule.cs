using UnityEngine;

public enum IngredientOrientationTarget
{
    Horizontal,
    Vertical,
    Diagonal
}

[CreateAssetMenu(
    fileName = "NewOrientationRule",
    menuName =
        "Burger Shake/Scoring Rules/Orientation"
)]
public class OrientationScoringRule :
    IngredientScoringRule
{
    [Header("Orientation")]
    public IngredientOrientationTarget orientation =
        IngredientOrientationTarget.Horizontal;

    [Range(0f, 45f)]
    [Tooltip(
        "How many degrees away from the target orientation still counts."
    )]
    public float angleTolerance = 15f;

    [Header("Optional Contact Requirement")]
    [Min(0)]
    public int minimumContacts = 0;

    [Tooltip(
        "-1 means there is no maximum."
    )]
    public int maximumContacts = -1;

    [Tooltip(
        "If enabled, Amount is awarded once for every touching ingredient after the orientation/contact condition is met."
    )]
    public bool rewardPerContact;

    public override ScoreValue Evaluate(
        Ingredient ingredient
    )
    {
        if (ingredient == null)
        {
            return default;
        }

        int contacts =
            ingredient.TouchingCount;

        if (contacts < minimumContacts)
        {
            return default;
        }

        if (
            maximumContacts >= 0 &&
            contacts > maximumContacts
        )
        {
            return default;
        }

        if (!MatchesOrientation(ingredient))
        {
            return default;
        }

        return CreateReward(
            rewardPerContact
                ? contacts
                : 1f
        );
    }

    private bool MatchesOrientation(
        Ingredient ingredient
    )
    {
        float normalized =
            Mathf.Repeat(
                ingredient.transform.eulerAngles.z,
                180f
            );

        float distance;

        switch (orientation)
        {
            case IngredientOrientationTarget.Horizontal:
                distance =
                    Mathf.Min(
                        normalized,
                        180f - normalized
                    );
                break;

            case IngredientOrientationTarget.Vertical:
                distance =
                    Mathf.Abs(
                        normalized - 90f
                    );
                break;

            case IngredientOrientationTarget.Diagonal:
                distance =
                    Mathf.Min(
                        Mathf.Abs(
                            normalized - 45f
                        ),
                        Mathf.Abs(
                            normalized - 135f
                        )
                    );
                break;

            default:
                return false;
        }

        return distance <= angleTolerance;
    }
}
