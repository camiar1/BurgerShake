using System;
using UnityEngine;

public class CustomerChallengeController : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GameplayModifiers gameplayModifiers;
    [SerializeField] private IngredientDropper ingredientDropper;
    [SerializeField] private Transform blenderRoot;

    public CustomerDefinition CurrentCustomer { get; private set; }
    public int GoalScore { get; private set; }

    public event Action<CustomerDefinition, int> ChallengeStarted;
    public event Action<bool, int> ChallengeFinished;

    public void BeginChallenge(CustomerDefinition customer, int goalScore)
    {
        CurrentCustomer = customer;
        GoalScore = Mathf.Max(1, goalScore);

        gameplayModifiers?.Apply(customer != null ? customer.restrictions : null);
        ingredientDropper?.ResetChallenge();
        ApplyBlenderScale();

        ChallengeStarted?.Invoke(CurrentCustomer, GoalScore);
    }

    public bool CompleteChallenge()
    {
        if (CurrentCustomer == null || scoreManager == null)
        {
            return false;
        }

        scoreManager.RecalculateScore();
        Ingredient[] ingredients = FindObjectsByType<Ingredient>(FindObjectsSortMode.None);

        bool passed = scoreManager.TotalScore >= GoalScore;
        int earnedCoins = passed ? CurrentCustomer.baseRewardCoins : 0;

        if (passed)
        {
            foreach (CustomerPreference preference in CurrentCustomer.preferences)
            {
                if (preference != null && preference.IsSatisfied(GoalScore, scoreManager.TotalScore, ingredients))
                {
                    earnedCoins += preference.bonusCoins;
                }
            }
        }

        ChallengeFinished?.Invoke(passed, earnedCoins);
        return passed;
    }

    private void ApplyBlenderScale()
    {
        if (blenderRoot == null)
        {
            return;
        }

        float scale = gameplayModifiers != null ? gameplayModifiers.BlenderScale : 1f;
        Vector3 current = blenderRoot.localScale;
        blenderRoot.localScale = new Vector3(scale, scale, current.z);
    }
}
