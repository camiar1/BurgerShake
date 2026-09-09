using UnityEngine;

public class FinishShakeButtonVisibility :
    MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject finishShakeButton;

    [SerializeField]
    private CatTossDraftController catTossDraftController;

    [SerializeField]
    private IngredientDraftManager draftManager;

    [SerializeField]
    private IngredientDropper ingredientDropper;

    [SerializeField]
    private RunManager runManager;

    private bool roundHasStarted;

    private void Awake()
    {
        if (catTossDraftController == null)
        {
            catTossDraftController =
                FindFirstObjectByType<
                    CatTossDraftController
                >();
        }

        if (draftManager == null)
        {
            draftManager =
                FindFirstObjectByType<
                    IngredientDraftManager
                >();
        }

        if (ingredientDropper == null)
        {
            ingredientDropper =
                FindFirstObjectByType<
                    IngredientDropper
                >();
        }

        if (runManager == null)
        {
            runManager =
                FindFirstObjectByType<
                    RunManager
                >();
        }

        SetButtonVisible(
            false
        );
    }

    private void Update()
    {
        RefreshVisibility();
    }

    private void RefreshVisibility()
    {
        bool inAssembly =
            runManager == null ||
            runManager.State ==
                RunState.Assembly;

        if (!inAssembly)
        {
            roundHasStarted =
                false;

            SetButtonVisible(
                false
            );

            return;
        }

        if (
            catTossDraftController ==
            null
        )
        {
            SetButtonVisible(
                false
            );

            return;
        }

        // Once BeginRound gives the cat
        // its dispenses, we know gameplay
        // has actually started.
        if (
            catTossDraftController
                .DispensesRemaining >
            0
        )
        {
            roundHasStarted =
                true;
        }

        if (!roundHasStarted)
        {
            SetButtonVisible(
                false
            );

            return;
        }

        bool allDispensesUsed =
            catTossDraftController
                .DispensesRemaining <=
            0;

        bool noChoicesRemaining =
            draftManager == null ||
            draftManager
                .CurrentChoices
                .Count ==
            0;

        bool noIngredientWaitingToDrop =
            ingredientDropper == null ||
            !ingredientDropper
                .HasIngredient;

        bool shouldShow =
            allDispensesUsed &&
            noChoicesRemaining &&
            noIngredientWaitingToDrop;

        SetButtonVisible(
            shouldShow
        );
    }

    private void SetButtonVisible(
        bool visible
    )
    {
        if (
            finishShakeButton ==
            null
        )
        {
            return;
        }

        if (
            finishShakeButton
                .activeSelf ==
            visible
        )
        {
            return;
        }

        finishShakeButton.SetActive(
            visible
        );
    }
}