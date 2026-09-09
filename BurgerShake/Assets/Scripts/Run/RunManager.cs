using System;
using System.Collections;
using UnityEngine;

public enum RunState
{
    Setup,
    CustomerIntro,
    Assembly,
    ScoreReveal,
    CustomerOutro,
    Shop,
    Won,
    Lost
}

public class RunManager : MonoBehaviour
{
    [Header("Run")]
    [SerializeField]
    private RunDefinition runDefinition;

    [SerializeField]
    private RunProgress progress;

    [Header("Starting Pantry")]
    [Tooltip(
        "Used when Auto Start Run is enabled. " +
        "Later the pantry selection screen will pass " +
        "a pantry directly instead."
    )]
    [SerializeField]
    private StartingPantryDefinition
        defaultStartingPantry;

    [SerializeField]
    private bool autoStartRun = true;

    [Header("Gameplay")]
    [SerializeField]
    private IngredientDraftManager
        draftManager;

    [SerializeField]
    private CustomerChallengeController
        challengeController;

    [SerializeField]
    private ScoreRevealController
        scoreRevealController;

    [Header("Views")]
    [SerializeField]
    private ViewController viewController;

    [SerializeField]
    private CustomerSpawner customerSpawner;

    [Header("Customer Intro")]
    [SerializeField]
    private float customerIntroDelay =
        0.5f;

    [Header("Customer Outro")]
    [SerializeField]
    private float leaveDelay =
        0.5f;

    [SerializeField]
    private float postLeaveDelay =
        0.3f;

    public RunState State
    {
        get;
        private set;
    } = RunState.Setup;

    public CustomerDefinition CurrentCustomer
    {
        get;
        private set;
    }

    public int CurrentGoalScore
    {
        get;
        private set;
    }

    public bool RunStarted
    {
        get;
        private set;
    }

    public StartingPantryDefinition
        SelectedStartingPantry
    {
        get;
        private set;
    }

    public event Action<RunState>
        StateChanged;

    public event Action<
        CustomerDefinition,
        int
    > CustomerIntroStarted;

    private bool waitingForCustomerWindow;
    private bool waitingForAssembly;
    private bool waitingForOutroWindow;

    private RunState stateAfterOutro;

    private Coroutine introRoutine;
    private Coroutine outroRoutine;
    private Coroutine scoreRevealRoutine;

    private void Awake()
    {
        if (progress == null)
        {
            progress =
                FindFirstObjectByType<
                    RunProgress
                >();
        }

        if (viewController == null)
        {
            viewController =
                FindFirstObjectByType<
                    ViewController
                >();
        }

        if (customerSpawner == null)
        {
            customerSpawner =
                FindFirstObjectByType<
                    CustomerSpawner
                >();
        }

        if (scoreRevealController == null)
        {
            scoreRevealController =
                FindFirstObjectByType<
                    ScoreRevealController
                >();
        }
    }

    private void OnEnable()
    {
        if (challengeController != null)
        {
            challengeController
                .ChallengeFinished +=
                    HandleChallengeFinished;
        }

        if (viewController != null)
        {
            viewController.ViewChanged +=
                HandleViewChanged;
        }

        if (customerSpawner != null)
        {
            customerSpawner.CustomerLeft +=
                HandleCustomerLeft;
        }
    }

    private void OnDisable()
    {
        if (challengeController != null)
        {
            challengeController
                .ChallengeFinished -=
                    HandleChallengeFinished;
        }

        if (viewController != null)
        {
            viewController.ViewChanged -=
                HandleViewChanged;
        }

        if (customerSpawner != null)
        {
            customerSpawner.CustomerLeft -=
                HandleCustomerLeft;
        }

        if (introRoutine != null)
        {
            StopCoroutine(
                introRoutine
            );

            introRoutine =
                null;
        }

        if (outroRoutine != null)
        {
            StopCoroutine(
                outroRoutine
            );

            outroRoutine =
                null;
        }

        if (scoreRevealRoutine != null)
        {
            StopCoroutine(
                scoreRevealRoutine
            );

            scoreRevealRoutine =
                null;
        }
    }

    private IEnumerator Start()
    {
        if (!autoStartRun)
        {
            yield break;
        }

        yield return null;

        StartRun();
    }

    public void StartRun()
    {
        StartRun(
            defaultStartingPantry
        );
    }

    public void StartRun(
        StartingPantryDefinition
            startingPantry
    )
    {
        if (RunStarted)
        {
            return;
        }

        if (runDefinition == null)
        {
            Debug.LogError(
                "RunManager has no RunDefinition."
            );

            return;
        }

        if (progress == null)
        {
            Debug.LogError(
                "RunManager has no RunProgress."
            );

            return;
        }

        if (startingPantry == null)
        {
            Debug.LogError(
                "RunManager has no Starting Pantry."
            );

            return;
        }

        if (
            startingPantry.ingredients ==
                null ||
            startingPantry.ingredients.Count ==
                0
        )
        {
            Debug.LogError(
                "The selected Starting Pantry has no ingredients."
            );

            return;
        }

        if (
            runDefinition.customers ==
                null ||
            runDefinition.customers.Count ==
                0
        )
        {
            Debug.LogError(
                "The RunDefinition has no customers."
            );

            return;
        }

        SelectedStartingPantry =
            startingPantry;

        RunStarted =
            true;

        progress.BeginRun(
            runDefinition,
            startingPantry
        );

        StartCurrentDay();
    }

    public void StartCurrentDay()
    {
        if (
            runDefinition == null ||
            progress == null
        )
        {
            return;
        }

        if (
            progress.Day >
            runDefinition.customers.Count
        )
        {
            SetState(
                RunState.Won
            );

            return;
        }

        int customerIndex =
            progress.Day - 1;

        CurrentCustomer =
            runDefinition.customers[
                customerIndex
            ];

        if (CurrentCustomer == null)
        {
            Debug.LogError(
                $"Customer for Day {progress.Day} is missing."
            );

            return;
        }

        float multiplier =
            runDefinition
                .goalMultiplierByDay
                .Evaluate(
                    progress.Day
                );

        CurrentGoalScore =
            Mathf.RoundToInt(
                CurrentCustomer
                    .baseGoalScore *
                multiplier
            );

        waitingForCustomerWindow =
            false;

        waitingForAssembly =
            false;

        waitingForOutroWindow =
            false;

        if (introRoutine != null)
        {
            StopCoroutine(
                introRoutine
            );

            introRoutine =
                null;
        }

        SetState(
            RunState.CustomerIntro
        );

        if (viewController == null)
        {
            BeginCustomerIntroDelay();

            return;
        }

        if (
            viewController.CurrentView ==
                ViewController
                    .FoodTruckView
                    .CustomerWindow &&
            !viewController.IsSliding
        )
        {
            BeginCustomerIntroDelay();

            return;
        }

        waitingForCustomerWindow =
            true;

        viewController
            .GoToCustomerWindow();
    }

    public void BeginCurrentCustomer()
    {
        if (
            State !=
                RunState.CustomerIntro ||
            CurrentCustomer == null
        )
        {
            return;
        }

        if (viewController == null)
        {
            StartAssemblyGameplay();

            return;
        }

        if (
            viewController.CurrentView ==
                ViewController
                    .FoodTruckView
                    .Assembly &&
            !viewController.IsSliding
        )
        {
            StartAssemblyGameplay();

            return;
        }

        waitingForAssembly =
            true;

        viewController.GoToAssembly();
    }

    public void FinishCurrentCustomer()
    {
        if (
            State !=
            RunState.Assembly
        )
        {
            return;
        }

        if (scoreRevealRoutine != null)
        {
            return;
        }

        scoreRevealRoutine =
            StartCoroutine(
                FinishCustomerRoutine()
            );
    }

    public void ContinueAfterShop()
    {
        if (
            State != RunState.Shop ||
            progress == null
        )
        {
            return;
        }

        progress.AdvanceDay();

        if (
            runDefinition != null &&
            progress.Day >
                runDefinition
                    .customers
                    .Count
        )
        {
            SetState(
                RunState.Won
            );

            return;
        }

        StartCurrentDay();
    }

    private IEnumerator
        FinishCustomerRoutine()
    {
        SetState(
            RunState.ScoreReveal
        );

        if (scoreRevealController != null)
        {
            yield return
                scoreRevealController
                    .PlayReveal();

            scoreRevealRoutine =
                null;

            challengeController
                ?.CompleteChallengeUsingCurrentScore();

            yield break;
        }

        scoreRevealRoutine =
            null;

        challengeController
            ?.CompleteChallenge();
    }

    private void HandleViewChanged(
        ViewController.FoodTruckView view
    )
    {
        if (
            waitingForCustomerWindow &&
            view ==
                ViewController
                    .FoodTruckView
                    .CustomerWindow
        )
        {
            waitingForCustomerWindow =
                false;

            BeginCustomerIntroDelay();

            return;
        }

        if (
            waitingForAssembly &&
            view ==
                ViewController
                    .FoodTruckView
                    .Assembly
        )
        {
            StartAssemblyGameplay();

            return;
        }

        if (
            waitingForOutroWindow &&
            view ==
                ViewController
                    .FoodTruckView
                    .CustomerWindow
        )
        {
            waitingForOutroWindow =
                false;

            StartCustomerLeaveSequence();
        }
    }

    private void BeginCustomerIntroDelay()
    {
        if (introRoutine != null)
        {
            StopCoroutine(
                introRoutine
            );
        }

        introRoutine =
            StartCoroutine(
                CustomerIntroDelayRoutine()
            );
    }

    private IEnumerator
        CustomerIntroDelayRoutine()
    {
        if (customerIntroDelay > 0f)
        {
            yield return
                new WaitForSecondsRealtime(
                    customerIntroDelay
                );
        }

        introRoutine =
            null;

        PresentCustomerIntro();
    }

    private void PresentCustomerIntro()
    {
        if (
            State !=
                RunState.CustomerIntro ||
            CurrentCustomer == null
        )
        {
            return;
        }

        CustomerIntroStarted?.Invoke(
            CurrentCustomer,
            CurrentGoalScore
        );
    }

    private void StartAssemblyGameplay()
    {
        waitingForAssembly =
            false;

        SetState(
            RunState.Assembly
        );

        draftManager
            ?.BeginRoundDraftCycle();

        challengeController
            ?.BeginChallenge(
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
            SetState(
                RunState.Lost
            );

            return;
        }

        progress?.AddCoins(
            earnedCoins
        );

        if (
            runDefinition != null &&
            progress != null &&
            progress.Day >=
                runDefinition
                    .customers
                    .Count
        )
        {
            stateAfterOutro =
                RunState.Won;
        }
        else
        {
            stateAfterOutro =
                RunState.Shop;
        }

        BeginCustomerOutro();
    }

    private void BeginCustomerOutro()
    {
        SetState(
            RunState.CustomerOutro
        );

        if (viewController == null)
        {
            StartCustomerLeaveSequence();

            return;
        }

        if (
            viewController.CurrentView ==
                ViewController
                    .FoodTruckView
                    .CustomerWindow &&
            !viewController.IsSliding
        )
        {
            StartCustomerLeaveSequence();

            return;
        }

        waitingForOutroWindow =
            true;

        viewController
            .GoToCustomerWindow();
    }

    private void StartCustomerLeaveSequence()
    {
        if (outroRoutine != null)
        {
            StopCoroutine(
                outroRoutine
            );
        }

        outroRoutine =
            StartCoroutine(
                CustomerLeaveSequence()
            );
    }

    private IEnumerator
        CustomerLeaveSequence()
    {
        if (leaveDelay > 0f)
        {
            yield return
                new WaitForSecondsRealtime(
                    leaveDelay
                );
        }

        if (customerSpawner != null)
        {
            customerSpawner
                .CustomerLeaves();

            yield break;
        }

        HandleCustomerLeft();
    }

    private void HandleCustomerLeft()
    {
        if (
            State !=
            RunState.CustomerOutro
        )
        {
            return;
        }

        if (outroRoutine != null)
        {
            StopCoroutine(
                outroRoutine
            );

            outroRoutine =
                null;
        }

        outroRoutine =
            StartCoroutine(
                FinishOutroAfterDelay()
            );
    }

    private IEnumerator
        FinishOutroAfterDelay()
    {
        if (postLeaveDelay > 0f)
        {
            yield return
                new WaitForSecondsRealtime(
                    postLeaveDelay
                );
        }

        outroRoutine =
            null;

        SetState(
            stateAfterOutro
        );
    }

    private void SetState(
        RunState state
    )
    {
        State =
            state;

        StateChanged?.Invoke(
            State
        );
    }
}