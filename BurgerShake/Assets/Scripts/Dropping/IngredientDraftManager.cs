using System.Collections.Generic;
using UnityEngine;

public class IngredientDraftManager :
    MonoBehaviour
{
    [Header("Run")]
    [SerializeField]
    private RunProgress progress;

    [Header("Draft")]
    [SerializeField]
    [Min(1)]
    private int defaultChoiceCount =
        3;

    [Tooltip(
        "Maximum copies of the same ingredient allowed " +
        "in one hand when another ingredient is available."
    )]
    [SerializeField]
    [Min(1)]
    private int maxSameIngredientPerHand =
        2;

    [Header("Legacy Button UI")]
    [Tooltip(
        "Optional. Leave this empty when using the cat toss system."
    )]
    [SerializeField]
    private IngredientChoiceButton[]
        choiceButtons;

    [Header("Gameplay")]
    [SerializeField]
    private IngredientDropper dropper;

    [SerializeField]
    private GameplayModifiers
        gameplayModifiers;

    private readonly List<
        IngredientDefinition
    > drawBag =
        new List<
            IngredientDefinition
        >();

    private readonly List<
        IngredientDefinition
    > discardBag =
        new List<
            IngredientDefinition
        >();

    private readonly List<
        IngredientDefinition
    > currentChoices =
        new List<
            IngredientDefinition
        >();

    private int cycleNumber =
        1;

    public IReadOnlyList<
        IngredientDefinition
    > CurrentChoices =>
        currentChoices;

    public int DrawBagCount =>
        drawBag.Count;

    public int DiscardBagCount =>
        discardBag.Count;

    public int CurrentCycle =>
        cycleNumber;

    private void Awake()
    {
        if (progress == null)
        {
            progress =
                FindFirstObjectByType<
                    RunProgress
                >();
        }

        if (dropper != null)
        {
            dropper.Initialize(
                this
            );
        }
    }

    public void BeginRoundDraftCycle()
    {
        currentChoices.Clear();
        drawBag.Clear();
        discardBag.Clear();

        cycleNumber =
            1;

        FillDrawBagFromPantry();

        UpdateLegacyButtons();

        SetChoiceButtonsInteractable(
            false
        );
    }

    public void RefreshChoices()
    {
        currentChoices.Clear();

        int requestedChoices =
            gameplayModifiers != null
                ? gameplayModifiers
                    .DraftChoiceCount
                : defaultChoiceCount;

        requestedChoices =
            Mathf.Max(
                1,
                requestedChoices
            );

        for (
            int i = 0;
            i < requestedChoices;
            i++
        )
        {
            if (!EnsureDrawAvailable())
            {
                break;
            }

            int index =
                RollWeightedBagIndexForHand();

            if (
                index < 0 ||
                index >= drawBag.Count
            )
            {
                break;
            }

            IngredientDefinition chosen =
                drawBag[index];

            currentChoices.Add(
                chosen
            );

            drawBag.RemoveAt(
                index
            );

            discardBag.Add(
                chosen
            );
        }

        UpdateLegacyButtons();

        SetChoiceButtonsInteractable(
            true
        );
    }

    public void SelectIngredient(
        IngredientDefinition ingredient
    )
    {
        if (
            ingredient == null ||
            !currentChoices.Contains(
                ingredient
            ) ||
            dropper == null
        )
        {
            return;
        }

        dropper.SetIngredient(
            ingredient
        );

        SetChoiceButtonsInteractable(
            false
        );
    }

    public void IngredientWasDropped()
    {
        ClearCurrentChoices();
    }

    public void ClearCurrentChoices()
    {
        currentChoices.Clear();

        UpdateLegacyButtons();

        SetChoiceButtonsInteractable(
            false
        );
    }

    public int GetCopiesRemainingInDrawBag(
        IngredientDefinition ingredient
    )
    {
        if (ingredient == null)
        {
            return 0;
        }

        int count =
            0;

        foreach (
            IngredientDefinition entry
            in drawBag
        )
        {
            if (entry == ingredient)
            {
                count++;
            }
        }

        return count;
    }

    private void FillDrawBagFromPantry()
    {
        if (progress == null)
        {
            return;
        }

        foreach (
            RunIngredientEntry entry
            in progress.Pantry
        )
        {
            if (
                entry == null ||
                entry.Ingredient == null ||
                entry.Copies <= 0
            )
            {
                continue;
            }

            for (
                int i = 0;
                i < entry.Copies;
                i++
            )
            {
                drawBag.Add(
                    entry.Ingredient
                );
            }
        }
    }

    private bool EnsureDrawAvailable()
    {
        if (drawBag.Count > 0)
        {
            return true;
        }

        if (discardBag.Count == 0)
        {
            return false;
        }

        drawBag.AddRange(
            discardBag
        );

        discardBag.Clear();

        cycleNumber++;

        return
            drawBag.Count > 0;
    }

    private int RollWeightedBagIndexForHand()
    {
        if (drawBag.Count == 0)
        {
            return -1;
        }

        List<int> eligibleIndices =
            new List<int>();

        for (
            int i = 0;
            i < drawBag.Count;
            i++
        )
        {
            IngredientDefinition ingredient =
                drawBag[i];

            if (ingredient == null)
            {
                continue;
            }

            int alreadyInHand =
                CountIngredientInCurrentChoices(
                    ingredient
                );

            if (
                alreadyInHand <
                maxSameIngredientPerHand
            )
            {
                eligibleIndices.Add(
                    i
                );
            }
        }

        // If every remaining ingredient would
        // exceed the limit, allow it rather than
        // creating a dead end.
        if (eligibleIndices.Count == 0)
        {
            for (
                int i = 0;
                i < drawBag.Count;
                i++
            )
            {
                if (drawBag[i] != null)
                {
                    eligibleIndices.Add(
                        i
                    );
                }
            }
        }

        return
            RollWeightedIndexFromCandidates(
                eligibleIndices
            );
    }

    private int
        RollWeightedIndexFromCandidates(
            List<int> candidateIndices
        )
    {
        if (
            candidateIndices == null ||
            candidateIndices.Count == 0
        )
        {
            return -1;
        }

        float totalWeight =
            0f;

        foreach (
            int index
            in candidateIndices
        )
        {
            IngredientDefinition ingredient =
                drawBag[index];

            if (ingredient == null)
            {
                continue;
            }

            totalWeight +=
                Mathf.Max(
                    0.01f,
                    ingredient
                        .draftWeight
                );
        }

        if (totalWeight <= 0f)
        {
            int randomCandidate =
                Random.Range(
                    0,
                    candidateIndices.Count
                );

            return
                candidateIndices[
                    randomCandidate
                ];
        }

        float roll =
            Random.Range(
                0f,
                totalWeight
            );

        foreach (
            int index
            in candidateIndices
        )
        {
            IngredientDefinition ingredient =
                drawBag[index];

            if (ingredient == null)
            {
                continue;
            }

            roll -=
                Mathf.Max(
                    0.01f,
                    ingredient
                        .draftWeight
                );

            if (roll <= 0f)
            {
                return index;
            }
        }

        return
            candidateIndices[
                candidateIndices.Count - 1
            ];
    }

    private int CountIngredientInCurrentChoices(
        IngredientDefinition ingredient
    )
    {
        if (ingredient == null)
        {
            return 0;
        }

        int count =
            0;

        foreach (
            IngredientDefinition choice
            in currentChoices
        )
        {
            if (choice == ingredient)
            {
                count++;
            }
        }

        return count;
    }

    private void UpdateLegacyButtons()
    {
        if (
            choiceButtons == null ||
            choiceButtons.Length == 0
        )
        {
            return;
        }

        int visibleCount =
            Mathf.Min(
                currentChoices.Count,
                choiceButtons.Length
            );

        for (
            int i = 0;
            i < visibleCount;
            i++
        )
        {
            if (
                choiceButtons[i] ==
                null
            )
            {
                continue;
            }

            choiceButtons[i].Setup(
                currentChoices[i],
                this
            );

            choiceButtons[i]
                .gameObject
                .SetActive(
                    true
                );
        }

        for (
            int i = visibleCount;
            i < choiceButtons.Length;
            i++
        )
        {
            if (
                choiceButtons[i] !=
                null
            )
            {
                choiceButtons[i]
                    .gameObject
                    .SetActive(
                        false
                    );
            }
        }
    }

    private void
        SetChoiceButtonsInteractable(
            bool interactable
        )
    {
        if (choiceButtons == null)
        {
            return;
        }

        foreach (
            IngredientChoiceButton
            choiceButton
            in choiceButtons
        )
        {
            if (choiceButton != null)
            {
                choiceButton
                    .SetInteractable(
                        interactable
                    );
            }
        }
    }
}