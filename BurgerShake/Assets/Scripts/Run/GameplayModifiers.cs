using System;
using System.Collections.Generic;
using UnityEngine;

public class GameplayModifiers : MonoBehaviour
{
    public float BlenderScale { get; private set; } = 1f;
    public float IngredientScale { get; private set; } = 1f;
    public int DraftChoiceCount { get; private set; } = 3;
    public int DropLimit { get; private set; } = -1;

    public event Action Changed;

    public void ResetToDefaults()
    {
        BlenderScale = 1f;
        IngredientScale = 1f;
        DraftChoiceCount = 3;
        DropLimit = -1;
        Changed?.Invoke();
    }

    public void Apply(IEnumerable<CustomerRestriction> restrictions)
    {
        ResetToDefaults();

        if (restrictions == null)
        {
            return;
        }

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

        Changed?.Invoke();
    }
}
