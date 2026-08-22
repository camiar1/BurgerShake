using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRunDefinition", menuName = "Burger Shake/Run Definition")]
public class RunDefinition : ScriptableObject
{
    [Header("Starting Pool")]
    [Min(1)] public int startingIngredientCount = 3;
    public List<IngredientDefinition> startingIngredients = new List<IngredientDefinition>();

    [Header("Customers")]
    public List<CustomerDefinition> customers = new List<CustomerDefinition>();

    [Header("Difficulty")]
    [Tooltip("Multiplies each customer's base goal by day index. X=day number, Y=goal multiplier.")]
    public AnimationCurve goalMultiplierByDay = AnimationCurve.Linear(1f, 1f, 10f, 2f);

    [Header("Economy")]
    [Min(0)] public int startingCoins = 0;
}
