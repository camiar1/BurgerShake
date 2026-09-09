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

    [Tooltip(
        "If enabled, Amount is awarded once " +
        "for every touching ingredient."
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
            ingredient.TouchingCount;

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