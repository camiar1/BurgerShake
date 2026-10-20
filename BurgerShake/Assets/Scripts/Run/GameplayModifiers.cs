using System;
using System.Collections.Generic;
using UnityEngine;

public class GameplayModifiers : MonoBehaviour
{
    [Header("Defaults")]
    [SerializeField] private float defaultBlenderScale = 1f;
    [SerializeField] private float defaultIngredientScale = 1f;
    [SerializeField] private int defaultDraftChoiceCount = 3;
    [SerializeField] private int defaultDropLimit = -1;

    public float BlenderScale { get; private set; } = 1f;
    public float IngredientScale { get; private set; } = 1f;
    public int DraftChoiceCount { get; private set; } = 3;
    public int DropLimit { get; private set; } = -1;

    public event Action Changed;

    private void Awake()
    {
        ResetToDefaults();
    }

    public void ResetToDefaults()
    {
        BlenderScale = defaultBlenderScale;
        IngredientScale = defaultIngredientScale;
        DraftChoiceCount = defaultDraftChoiceCount;
        DropLimit = defaultDropLimit;

        Changed?.Invoke();
    }

    public void Apply(IEnumerable<CustomerRestriction> restrictions)
    {
        ResetToDefaults();

        if (restrictions != null)
        {
            foreach (CustomerRestriction restriction in restrictions)
            {
                if (restriction == null)
                {
                    continue;
                }

                switch (restriction.type)
                {
                    case CustomerRestrictionType.BlenderScale:
                        BlenderScale *= Mathf.Max(0.1f, restriction.floatValue);
                        break;
                    case CustomerRestrictionType.IngredientScale:
                        IngredientScale *= Mathf.Max(0.1f, restriction.floatValue);
                        break;
                    case CustomerRestrictionType.DraftChoiceCount:
                        DraftChoiceCount = Mathf.Max(1, restriction.intValue);
                        break;
                    case CustomerRestrictionType.DropLimit:
                        DropLimit = Mathf.Max(1, restriction.intValue);
                        break;
                }
            }
        }

        Changed?.Invoke();
    }

    public void ApplyRunUpgrades(IEnumerable<RunUpgradeDefinition> upgrades)
    {
        if (upgrades == null)
        {
            return;
        }

        foreach (RunUpgradeDefinition upgrade in upgrades)
        {
            if (upgrade == null)
            {
                continue;
            }

            switch (upgrade.effectType)
            {
                case RunUpgradeEffectType.DraftChoiceBonus:
                    DraftChoiceCount += Mathf.RoundToInt(upgrade.amount);
                    break;
                case RunUpgradeEffectType.IngredientScaleMultiplier:
                    IngredientScale *= Mathf.Max(0.1f, upgrade.amount);
                    break;
            }
        }

        DraftChoiceCount = Mathf.Max(1, DraftChoiceCount);
        Changed?.Invoke();
    }
}
