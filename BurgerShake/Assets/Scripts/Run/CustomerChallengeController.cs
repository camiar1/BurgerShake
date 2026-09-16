using System;
using UnityEngine;

public class CustomerChallengeController :
    MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField]
    private ScoreManager scoreManager;

    [SerializeField]
    private GameplayModifiers
        gameplayModifiers;

    [SerializeField]
    private UpgradeManager upgradeManager;

    [SerializeField]
    private IngredientDropper
        ingredientDropper;

    [Header("Assembly")]
    [SerializeField]
    private Transform blenderRoot;

    [SerializeField]
    private RectTransform blenderVisual;

    [SerializeField]
    private Transform ingredientContainer;

    public CustomerDefinition CurrentCustomer
    {
        get;
        private set;
    }

    public int GoalScore
    {
        get;
        private set;
    }

    public event Action<
        CustomerDefinition,
        int
    > ChallengeStarted;

    public event Action<
        bool,
        int
    > ChallengeFinished;

    private Vector3 blenderRootBaseScale =
        Vector3.one;

    private Vector3 blenderVisualBaseScale =
        Vector3.one;

    private bool blenderRootScaleCaptured;
    private bool blenderVisualScaleCaptured;

    private void Awake()
    {
        ResolveBlenderVisual();
        CaptureBlenderBaseScales();
    }

    public void BeginChallenge(
        CustomerDefinition customer,
        int goalScore
    )
    {
        // Safety cleanup in case anything
        // somehow survived the previous round.
        ClearIngredients();

        CurrentCustomer =
            customer;

        GoalScore =
            Mathf.Max(
                1,
                goalScore
            );

        gameplayModifiers?.Apply(
            customer != null
                ? customer.restrictions
                : null
        );

        upgradeManager
            ?.ApplyOwnedUpgrades();

        ingredientDropper
            ?.ResetChallenge();

        ApplyBlenderScale();

        scoreManager
            ?.ResetScore();

        ChallengeStarted?.Invoke(
            CurrentCustomer,
            GoalScore
        );
    }

    public bool CompleteChallenge()
    {
        if (scoreManager == null)
        {
            return false;
        }

        scoreManager
            .CalculateFinalScore();

        return
            CompleteChallengeUsingCurrentScore();
    }

    public bool
        CompleteChallengeUsingCurrentScore()
    {
        if (
            CurrentCustomer == null ||
            scoreManager == null
        )
        {
            return false;
        }

        // We need these ingredients to remain
        // alive until AFTER customer preferences
        // have been evaluated.
        Ingredient[] ingredients =
            GetChallengeIngredients();

        bool passed =
            scoreManager.TotalScore >=
            GoalScore;

        int earnedCoins =
            passed
                ? CurrentCustomer
                    .baseRewardCoins
                : 0;

        if (passed)
        {
            if (upgradeManager != null)
            {
                earnedCoins +=
                    upgradeManager
                        .BonusCoinsPerWin;
            }

            foreach (
                CustomerPreference preference
                in CurrentCustomer.preferences
            )
            {
                if (
                    preference != null &&
                    preference.IsSatisfied(
                        GoalScore,
                        scoreManager.TotalScore,
                        ingredients
                    )
                )
                {
                    earnedCoins +=
                        preference.bonusCoins;
                }
            }
        }

        // Let RunManager and any other listeners
        // receive the finished result while all
        // challenge data is still intact.
        ChallengeFinished?.Invoke(
            passed,
            earnedCoins
        );

        // At this point:
        //
        // - final score is complete
        // - customer preferences are complete
        // - earned coins are complete
        // - ChallengeFinished has fired
        //
        // Nothing should need the physical
        // ingredient objects anymore.
        ClearIngredients();

        return passed;
    }

    private Ingredient[]
        GetChallengeIngredients()
    {
        if (ingredientContainer != null)
        {
            return ingredientContainer
                .GetComponentsInChildren<
                    Ingredient
                >(
                    false
                );
        }

        return FindObjectsByType<
            Ingredient
        >(
            FindObjectsSortMode.None
        );
    }

    private void ClearIngredients()
    {
        if (ingredientContainer == null)
        {
            return;
        }

        for (
            int i =
                ingredientContainer.childCount -
                1;
            i >= 0;
            i--
        )
        {
            GameObject ingredient =
                ingredientContainer
                    .GetChild(i)
                    .gameObject;

            // Hide immediately.
            //
            // Destroy() itself finishes at the
            // end of the frame, so disabling
            // first prevents even a single-frame
            // flash.
            ingredient.SetActive(
                false
            );

            Destroy(
                ingredient
            );
        }
    }

    private void ApplyBlenderScale()
    {
        ResolveBlenderVisual();
        CaptureBlenderBaseScales();

        float widthScale =
            gameplayModifiers != null
                ? gameplayModifiers
                    .BlenderScale
                : 1f;

        widthScale =
            Mathf.Max(
                0.1f,
                widthScale
            );

        // Blender restrictions change the usable
        // width, not the height. Keeping Y at its
        // original value makes the blender look
        // squeezed/widened rather than uniformly
        // shrinking the whole object.
        if (blenderRoot != null)
        {
            blenderRoot.localScale =
                new Vector3(
                    blenderRootBaseScale.x *
                        widthScale,
                    blenderRootBaseScale.y,
                    blenderRootBaseScale.z
                );
        }

        // BlenderFront is a visual sibling of the
        // physics BlenderRoot in the current scene,
        // so it must be resized separately to stay
        // aligned with the collider width.
        if (blenderVisual != null)
        {
            blenderVisual.localScale =
                new Vector3(
                    blenderVisualBaseScale.x *
                        widthScale,
                    blenderVisualBaseScale.y,
                    blenderVisualBaseScale.z
                );
        }
    }

    private void ResolveBlenderVisual()
    {
        if (
            blenderVisual != null ||
            blenderRoot == null ||
            blenderRoot.parent == null
        )
        {
            return;
        }

        Transform visual =
            blenderRoot.parent.Find(
                "BlenderFront"
            );

        if (visual != null)
        {
            blenderVisual =
                visual as RectTransform;
        }
    }

    private void CaptureBlenderBaseScales()
    {
        if (
            blenderRoot != null &&
            !blenderRootScaleCaptured
        )
        {
            blenderRootBaseScale =
                blenderRoot.localScale;

            blenderRootScaleCaptured =
                true;
        }

        if (
            blenderVisual != null &&
            !blenderVisualScaleCaptured
        )
        {
            blenderVisualBaseScale =
                blenderVisual.localScale;

            blenderVisualScaleCaptured =
                true;
        }
    }
}
