using System;
using UnityEngine;

public enum RunState
{
    Setup,
    Customer,
    Shop,
    Won,
    Lost
}

public class RunManager : MonoBehaviour
{
    [SerializeField] private RunDefinition runDefinition;
    [SerializeField] private RunProgress progress;
    [SerializeField] private IngredientDraftManager draftManager;
    [SerializeField] private CustomerChallengeController challengeController;

    public RunState State { get; private set; } = RunState.Setup;
    public CustomerDefinition CurrentCustomer { get; private set; }

    public event Action<RunState> StateChanged;

    private void OnEnable()
    {
        if (challengeController != null)
        {
            challengeController.ChallengeFinished += HandleChallengeFinished;
        }
    }

    private void OnDisable()
    {
        if (challengeController != null)
        {
            challengeController.ChallengeFinished -= HandleChallengeFinished;
        }
    }

    public void StartRun()
    {
        if (runDefinition == null || progress == null)
        {
            Debug.LogError("RunManager is missing its RunDefinition or RunProgress reference.");
            return;
        }

        progress.BeginRun(runDefinition);
        draftManager?.SetIngredientPool(progress.Ingredients);
        StartCurrentDay();
    }

    public void StartCurrentDay()
    {
        if (runDefinition == null || progress == null || runDefinition.customers.Count == 0)
        {
            return;
        }

        int customerIndex = Mathf.Clamp(progress.Day - 1, 0, runDefinition.customers.Count - 1);
        CurrentCustomer = runDefinition.customers[customerIndex];

        float multiplier = runDefinition.goalMultiplierByDay.Evaluate(progress.Day);
        int goal = Mathf.RoundToInt(CurrentCustomer.baseGoalScore * multiplier);

        SetState(RunState.Customer);
        challengeController?.BeginChallenge(CurrentCustomer, goal);
    }

    public void FinishCurrentCustomer()
    {
        challengeController?.CompleteChallenge();
    }

    public void ContinueAfterShop()
    {
        if (State != RunState.Shop || progress == null)
        {
            return;
        }

        progress.AdvanceDay();

        if (runDefinition != null && progress.Day > runDefinition.customers.Count)
        {
            SetState(RunState.Won);
            return;
        }

        draftManager?.SetIngredientPool(progress.Ingredients);
        StartCurrentDay();
    }

    private void HandleChallengeFinished(bool passed, int earnedCoins)
    {
        if (!passed)
        {
            SetState(RunState.Lost);
            return;
        }

        progress?.AddCoins(earnedCoins);

        if (runDefinition != null && progress != null && progress.Day >= runDefinition.customers.Count)
        {
            SetState(RunState.Won);
        }
        else
        {
            SetState(RunState.Shop);
        }
    }

    private void SetState(RunState state)
    {
        State = state;
        StateChanged?.Invoke(State);
    }
}
