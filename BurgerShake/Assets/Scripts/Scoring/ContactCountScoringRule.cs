using UnityEngine;

[CreateAssetMenu(
    fileName = "NewContactCountRule",
    menuName =
        "Burger Shake/Scoring Rules/Contact Count"
)]
public class ContactCountScoringRule :
    IngredientScoringRule
{
    [Header("Contact Requirement")]
    [Min(0)]
    public int minimumContacts = 0;

    [Tooltip(
        "-1 means there is no maximum."
    )]
    public int maximumContacts = -1;

    [Header("Optional Tag Filter")]
    [Tooltip(
        "When enabled, only touching ingredients with Contact Tag are counted."
    )]
    public bool filterByTag;

    public IngredientTag contactTag;

    [Header("Reward")]
    [Tooltip(
        "If enabled, Amount is awarded once " +
        "for every counted touching ingredient."
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

        int count =
            filterByTag
                ? ingredient.CountTouchingWithTag(
                    contactTag
                )
                : ingredient.TouchingCount;

        if (count < minimumContacts)
        {
            return default;
        }

        if (
            maximumContacts >= 0 &&
            count > maximumContacts
        )
        {
            return default;
        }

        return CreateReward(
            rewardPerContact
                ? count
                : 1f
        );
    }
}
