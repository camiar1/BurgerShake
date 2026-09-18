using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private RunProgress progress;
    [SerializeField] private GameplayModifiers gameplayModifiers;
    [SerializeField] private ScoreManager scoreManager;

    public int BonusCoinsPerWin { get; private set; }

    public void ApplyOwnedUpgrades()
    {
        BonusCoinsPerWin = 0;
        float startingMultBonus = 0f;
        int startingPointsBonus = 0;

        if (progress == null)
        {
            scoreManager?.SetStartingMultBonus(0f);
            scoreManager?.SetStartingPointsBonus(0);
            return;
        }

        gameplayModifiers?.ApplyRunUpgrades(progress.Upgrades);

        foreach (RunUpgradeDefinition upgrade in progress.Upgrades)
        {
            if (upgrade == null)
            {
                continue;
            }

            switch (upgrade.effectType)
            {
                case RunUpgradeEffectType.StartingMultBonus:
                    startingMultBonus += upgrade.amount;
                    break;
                case RunUpgradeEffectType.BonusCoinsPerWin:
                    BonusCoinsPerWin += Mathf.RoundToInt(upgrade.amount);
                    break;
                case RunUpgradeEffectType.StartingPointsBonus:
                    startingPointsBonus += Mathf.Max(0, Mathf.RoundToInt(upgrade.amount));
                    break;
            }
        }

        scoreManager?.SetStartingMultBonus(startingMultBonus);
        scoreManager?.SetStartingPointsBonus(startingPointsBonus);
    }
}
