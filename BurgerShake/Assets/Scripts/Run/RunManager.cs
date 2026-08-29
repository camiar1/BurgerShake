using System;
using UnityEngine;

public enum RunState
{
    Setup,
    CustomerIntro,
    Assembly,
    Shop,
    Won,
    Lost
}

public class RunManager : MonoBehaviour
{
    [Header("Run")]
    [SerializeField] private RunDefinition runDefinition;
    [SerializeField] private RunProgress progress;

    [Header("Gameplay")]
    [SerializeField] private IngredientDraftManager draftManager;
    [SerializeField] private CustomerChallengeController challengeController;

    [Header("Views")]
    [SerializeField] private ViewController viewController;

    public RunState State { get; private set; } = RunState.Setup;

    public CustomerDefinition CurrentCustomer { get; private set; }

    public int CurrentGoalScore { get; private set; }

    public event Action<RunState> StateChanged;

    public event Action<CustomerDefinition, int> CustomerIntroStarted;

    private bool waitingForAssembly;

    private void OnEnable()
    {
        if (challengeController != null)
        {
            challengeController.ChallengeFinished += HandleChallengeFinished;
        }

        if (viewController != null)
        {
            viewController.ViewChanged += HandleViewChanged;
        }
    }

    private void OnDisable()
    {
        if (challengeController != null)
        {
            challengeController.ChallengeFinished -= HandleChallengeFinished;
        }

        if (viewController != null)
        {
            viewController.ViewChanged -= HandleViewChanged;
        }
    }

    public void StartRun()
    {
        if (runDefinition == null || progress == null)
        {
            Debug.LogError(
                "RunManager is missing its RunDefinition or RunProgress reference."
            );

            return;
        }

        progress.BeginRun(runDefinition);

        draftManager?.SetIngredientPool(progress.Ingredients);

        StartCurrentDay();
    }

    public void StartCurrentDay()
    {
        if (
            runDefinition == null ||
            progress == null ||
            runDefinition.customers.Count == 0
        )
        {
            return;
        }

        int customerIndex = Mathf.Clamp(
            progress.Day - 1,
            0,
            runDefinition.customers.Count - 1
        );

        CurrentCustomer = runDefinition.customers[customerIndex];

        float multiplier =
            runDefinition.goalMultiplierByDay.Evaluate(progress.Day);

        CurrentGoalScore = Mathf.RoundToInt(
            CurrentCustomer.baseGoalScore * multiplier
        );

        waitingForAssembly = false;

        SetState(RunState.CustomerIntro);

        viewController?.GoToCustomerWindow();

        CustomerIntroStarted?.Invoke(
            CurrentCustomer,
            CurrentGoalScore
        );
    }

    public void BeginCurrentCustomer()
    {
        if (State != RunState.CustomerIntro)
        {
            return;
        }

        if (CurrentCustomer == null)
        {
            return;
        }

        // No ViewController assigned:
        // just start gameplay immediately.
        if (viewController == null)
        {
            StartAssemblyGameplay();
            return;
        }

        // Already in Assembly for some reason.
        if (
            viewController.CurrentView ==
            ViewController.FoodTruckView.Assembly &&
            !viewController.IsSliding
        )
        {
            StartAssemblyGameplay();
            return;
        }

        waitingForAssembly = true;

        viewController.GoToAssembly();
    }

    public void FinishCurrentCustomer()
    {
        if (State != RunState.Assembly)
        {
            return;
        }

        challengeController?.CompleteChallenge();
    }

    public void ContinueAfterShop()
    {
        if (State != RunState.Shop || progress == null)
        {
            return;
        }

        progress.AdvanceDay();

        if (
            runDefinition != null &&
            progress.Day > runDefinition.customers.Count
        )
        {
            SetState(RunState.Won);
            return;
        }

        draftManager?.SetIngredientPool(progress.Ingredients);

        StartCurrentDay();
    }

    private void HandleViewChanged(
        ViewController.FoodTruckView view
    )
    {
        if (
            !waitingForAssembly ||
            view != ViewController.FoodTruckView.Assembly
        )
        {
            return;
        }

        StartAssemblyGameplay();
    }

    private void StartAssemblyGameplay()
    {
        waitingForAssembly = false;

        SetState(RunState.Assembly);

        challengeController?.BeginChallenge(
            CurrentCustomer,
            CurrentGoalScore
        );
    }

    private void HandleChallengeFinished(
        bool passed,
        int earnedCoins
    )
    {
        if (!passed)
        {
            SetState(RunState.Lost);
            return;
        }

        progress?.AddCoins(earnedCoins);

        if (
            runDefinition != null &&
            progress != null &&
            progress.Day >= runDefinition.customers.Count
        )
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