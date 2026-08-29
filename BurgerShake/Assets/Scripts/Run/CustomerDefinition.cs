using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCustomer", menuName = "Burger Shake/Customer")]
public class CustomerDefinition : ScriptableObject
{
    [Header("Identity")]
    public string customerName = "Customer";
    [TextArea] public string description;

    [Header("Visual")]
    public Sprite portrait;
    public Vector2 visualOffset = Vector2.zero;
    [Min(0.01f)] public float visualScale = 1f;

    [Header("Challenge")]
    [Min(1)] public int baseGoalScore = 100;
    [Min(0)] public int baseRewardCoins = 5;

    [Header("Restrictions")]
    public List<CustomerRestriction> restrictions =
        new List<CustomerRestriction>();

    [Header("Optional Wants")]
    public List<CustomerPreference> preferences =
        new List<CustomerPreference>();
}