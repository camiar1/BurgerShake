using System;
using UnityEngine;

public class CustomerChallengeController : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField]
    private ScoreManager scoreManager;

    [SerializeField]
    private GameplayModifiers gameplayModifiers;

    [SerializeField]
    private UpgradeManager upgradeManager;

    [SerializeField]
    private IngredientDropper ingredientDropper;

    [Header("Assembly")]
    [SerializeField]
    private Transform blenderRoot;

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

    public void BeginChallenge(
        CustomerDefinition customer,
        int goalScore
    )
    {
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

    public bool CompleteChallengeUsingCurrentScore()
    {
        if (
            CurrentCustomer == null ||
            scoreManager == null
        )
        {
            return false;
        }

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

        ChallengeFinished?.Invoke(
            passed,
            earnedCoins
        );

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
        if (blenderRoot == null)
        {
            return;
        }

        float scale =
            gameplayModifiers != null
                ? gameplayModifiers
                    .BlenderScale
                : 1f;

        Vector3 current =
            blenderRoot.localScale;

        blenderRoot.localScale =
            new Vector3(
                scale,
                scale,
                current.z
            );
    }
}