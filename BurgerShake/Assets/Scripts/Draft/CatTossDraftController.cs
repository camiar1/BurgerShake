using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatTossDraftController : MonoBehaviour
{
    public enum TossMode
    {
        HouseCat,
        Feral
    }

    [Header("Dependencies")]
    [SerializeField]
    private IngredientDraftManager draftManager;

    [SerializeField]
    private CatMascotView catMascotView;

    [SerializeField]
    private Transform ingredientContainer;

    [Header("UI Layout")]
    [SerializeField]
    private RectTransform choiceParent;

    [SerializeField]
    private RectTransform throwOrigin;

    [SerializeField]
    private RectTransform[] choiceSlots =
        new RectTransform[3];

    [SerializeField]
    private DraftChoiceVisual choicePrefab;

    [Header("Toss")]
    [SerializeField]
    private TossMode tossMode =
        TossMode.HouseCat;

    [SerializeField]
    private float tossTravelDuration =
        0.7f;

    [SerializeField]
    private float[] laneArcHeights =
        new float[3]
        {
            180f,
            230f,
            180f
        };

    [Header("Feral Hover")]
    [SerializeField]
    private float feralHoverAmplitude =
        14f;

    [SerializeField]
    private float feralHoverSpeed =
        1.8f;

    [Header("Timing")]
    [SerializeField]
    private float tossStartDelay =
        0.1f;

    [SerializeField]
    private float clearFadeDuration =
        0.15f;

    [SerializeField]
    private float nextTossDelayAfterDrop =
        0.2f;

    private readonly List<DraftChoiceVisual>
        activeChoices =
            new List<DraftChoiceVisual>();

    private bool waitingForPlacedIngredient;

    private bool tossInProgress;

    private int knownPlacedCount;

    private Coroutine
        delayedNextTossRoutine;

    private void Awake()
    {
        if (choiceParent == null)
        {
            choiceParent =
                transform as RectTransform;
        }

        if (ingredientContainer != null)
        {
            knownPlacedCount =
                ingredientContainer
                    .childCount;
        }
    }

    private void Update()
    {
        if (
            !waitingForPlacedIngredient ||
            ingredientContainer == null
        )
        {
            return;
        }

        int currentCount =
            ingredientContainer
                .childCount;

        if (
            currentCount >
            knownPlacedCount
        )
        {
            knownPlacedCount =
                currentCount;

            waitingForPlacedIngredient =
                false;

            if (
                delayedNextTossRoutine !=
                null
            )
            {
                StopCoroutine(
                    delayedNextTossRoutine
                );
            }

            delayedNextTossRoutine =
                StartCoroutine(
                    DelayedNextTossRoutine()
                );
        }
    }

    public void BeginRound()
    {
        ClearChoicesImmediate();

        if (ingredientContainer != null)
        {
            knownPlacedCount =
                ingredientContainer
                    .childCount;
        }

        waitingForPlacedIngredient =
            false;

        if (
            draftManager != null &&
            draftManager
                .CurrentChoices
                .Count == 0
        )
        {
            draftManager.RefreshChoices();
        }

        ShowNextToss();
    }

    public void SetTossMode(
        TossMode newMode
    )
    {
        tossMode =
            newMode;
    }

    public void ShowNextToss()
    {
        if (tossInProgress)
        {
            return;
        }

        StartCoroutine(
            ShowNextTossRoutine()
        );
    }

    private IEnumerator
        ShowNextTossRoutine()
    {
        tossInProgress =
            true;

        ClearChoicesImmediate();

        if (tossStartDelay > 0f)
        {
            yield return
                new WaitForSecondsRealtime(
                    tossStartDelay
                );
        }

        List<IngredientDefinition> offers =
            GetCurrentOffers();

        if (
            offers == null ||
            offers.Count == 0
        )
        {
            if (draftManager != null)
            {
                draftManager
                    .RefreshChoices();

                offers =
                    GetCurrentOffers();
            }
        }

        if (
            offers == null ||
            offers.Count == 0
        )
        {
            tossInProgress =
                false;

            yield break;
        }

        if (catMascotView != null)
        {
            catMascotView
                .PlayThrowPose();
        }

        int choiceCount =
            Mathf.Min(
                offers.Count,
                choiceSlots.Length
            );

        for (
            int i = 0;
            i < choiceCount;
            i++
        )
        {
            RectTransform slot =
                choiceSlots[i];

            if (
                slot == null ||
                choicePrefab == null ||
                throwOrigin == null
            )
            {
                continue;
            }

            IngredientDefinition definition =
                offers[i];

            DraftChoiceVisual choice =
                Instantiate(
                    choicePrefab,
                    choiceParent
                );

            choice.Setup(
                definition,
                GetIngredientSprite(
                    definition
                ),
                GetIngredientDisplayName(
                    definition
                ),
                HandleChoiceClicked
            );

            activeChoices.Add(
                choice
            );

            float arcHeight =
                i < laneArcHeights.Length
                    ? laneArcHeights[i]
                    : 180f;

            StartCoroutine(
                choice.FlyToSlot(
                    throwOrigin
                        .anchoredPosition,
                    slot
                        .anchoredPosition,
                    arcHeight,
                    tossTravelDuration,
                    tossMode ==
                        TossMode.HouseCat,
                    feralHoverAmplitude,
                    feralHoverSpeed,
                    i * 1.35f
                )
            );
        }

        tossInProgress =
            false;
    }

    private void HandleChoiceClicked(
        DraftChoiceVisual chosen
    )
    {
        if (
            chosen == null ||
            chosen.Definition == null
        )
        {
            return;
        }

        if (draftManager == null)
        {
            return;
        }

        // This is the real method in
        // your IngredientDraftManager.
        draftManager.SelectIngredient(
            chosen.Definition
        );

        waitingForPlacedIngredient =
            true;

        for (
            int i = 0;
            i < activeChoices.Count;
            i++
        )
        {
            DraftChoiceVisual choice =
                activeChoices[i];

            if (choice == null)
            {
                continue;
            }

            StartCoroutine(
                choice
                    .FadeOutAndDestroy(
                        clearFadeDuration
                    )
            );
        }

        activeChoices.Clear();
    }

    private IEnumerator
        DelayedNextTossRoutine()
    {
        if (
            nextTossDelayAfterDrop >
            0f
        )
        {
            yield return
                new WaitForSecondsRealtime(
                    nextTossDelayAfterDrop
                );
        }

        // IngredientDropper already calls
        // IngredientWasDropped(), which
        // refreshes CurrentChoices.
        ShowNextToss();

        delayedNextTossRoutine =
            null;
    }

    private List<IngredientDefinition>
        GetCurrentOffers()
    {
        if (draftManager == null)
        {
            return null;
        }

        List<IngredientDefinition> result =
            new List<IngredientDefinition>();

        foreach (
            IngredientDefinition ingredient
            in draftManager.CurrentChoices
        )
        {
            if (ingredient != null)
            {
                result.Add(
                    ingredient
                );
            }
        }

        return result;
    }

    private void ClearChoicesImmediate()
    {
        for (
            int i = 0;
            i < activeChoices.Count;
            i++
        )
        {
            if (activeChoices[i] != null)
            {
                Destroy(
                    activeChoices[i]
                        .gameObject
                );
            }
        }

        activeChoices.Clear();
    }

    private string
        GetIngredientDisplayName(
            IngredientDefinition definition
        )
    {
        if (definition == null)
        {
            return "";
        }

        return
            definition.ingredientName;
    }

    private Sprite GetIngredientSprite(
        IngredientDefinition definition
    )
    {
        if (definition == null)
        {
            return null;
        }

        if (definition.sprite != null)
        {
            return
                definition.sprite;
        }

        if (definition.prefab == null)
        {
            return null;
        }

        SpriteRenderer spriteRenderer =
            definition.prefab
                .GetComponentInChildren<
                    SpriteRenderer
                >();

        if (spriteRenderer != null)
        {
            return
                spriteRenderer.sprite;
        }

        return null;
    }
}