using UnityEngine;

public enum RunUpgradeEffectType
{
    DraftChoiceBonus,
    StartingMultBonus,
    IngredientScaleMultiplier,
    BonusCoinsPerWin
}

[CreateAssetMenu(fileName = "NewRunUpgrade", menuName = "Burger Shake/Run Upgrade")]
public class RunUpgradeDefinition : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;
    [Min(0)] public int cost = 5;

    public RunUpgradeEffectType effectType;
    public float amount = 1f;
}
