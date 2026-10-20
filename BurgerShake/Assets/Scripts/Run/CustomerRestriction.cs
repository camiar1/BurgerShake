using UnityEngine;

public enum CustomerRestrictionType
{
    BlenderScale,
    IngredientScale,
    DraftChoiceCount,
    DropLimit
}

[CreateAssetMenu(fileName = "NewCustomerRestriction", menuName = "Burger Shake/Customer Restriction")]
public class CustomerRestriction : ScriptableObject
{
    public string restrictionName;
    [TextArea] public string description;
    public CustomerRestrictionType type;
    public float floatValue = 1f;
    public int intValue = 0;
}
