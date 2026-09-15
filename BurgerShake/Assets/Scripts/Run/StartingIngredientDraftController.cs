using System;
using System.Collections.Generic;
using UnityEngine;

public class StartingIngredientDraftController :
    MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private RunManager runManager;

    [SerializeField]
    private RunDefinition runDefinition;

    [Header("Draft Rules")]
    [SerializeField]
    [Min(1)]
    private int rounds = 2;

    [SerializeField]
    [Min(1)]
    private int choicesPerRound = 3;

    [SerializeField]
    [Min(1)]
    private int copiesPerPick = 3;

    [SerializeField]
    [Min(0)]
    private int rerollsPerRun = 1;

    private readonly List<IngredientDefinition>
        currentChoices =
            new List<IngredientDefinition>();

    private readonly List<IngredientDefinition>
        selectedIngredients =
            new List<IngredientDefinition>();

    private readonly HashSet<IngredientDefinition>
        seenIngredients =
            new HashSet<IngredientDefinition>();

    public IReadOnlyList<IngredientDefinition>
        CurrentChoices =>
            currentChoices;

    public IReadOnlyList<IngredientDefinition>
        SelectedIngredients =>
            selectedIngredients;

    public int CurrentRound
    {
        get;
        private set;
    }

    public int RoundCount =>
        rounds;

    public int CopiesPerPick =>
        copiesPerPick;

    public int RerollsRemaining
    {
        get;
        private set;
    }

    public bool DraftActive
    {
        get;
        private set;
    }

    public event Action DraftStarted;
    public event Action ChoicesChanged;

    public event Action<IngredientDefinition>
        IngredientPicked;

    public event Action<
        IReadOnlyList<IngredientDefinition>
    > DraftCompleted;

    private void Awake()
    {
        if (runManager == null)
        {
            runManager =
                FindFirstObjectByType<
                    RunManager
                >();
        }

        if (
            runDefinition == null &&
            runManager != null
        )
        {
            runDefinition =
                runManager.Definition;
        }
    }

    public void BeginDraft()
    {
        if (DraftActive)
        {
            return;
        }

        if (
            runDefinition == null &&
            runManager != null
        )
        {
            runDefinition =
                runManager.Definition;
        }

        if (runDefinition == null)
        {
            Debug.LogError(
                "StartingIngredientDraftController has no RunDefinition."
            );

            return;
        }

        List<IngredientDefinition> eligible =
            GetValidStartingPool();

        if (eligible.Count < choicesPerRound)
        {
            Debug.LogError(
                $"Starting ingredient draft needs at least {choicesPerRound} valid ingredients."
            );

            return;
        }

        selectedIngredients.Clear();
        seenIngredients.Clear();
        currentChoices.Clear();

        CurrentRound = 1;
        RerollsRemaining =
            Mathf.Max(
                0,
                rerollsPerRun
            );

        DraftActive = true;

        FillRoundOneChoices();

        DraftStarted?.Invoke();
        ChoicesChanged?.Invoke();
    }

    public void Choose(
        int slotIndex
    )
    {
        if (
            !DraftActive ||
            slotIndex < 0 ||
            slotIndex >= currentChoices.Count
        )
        {
            return;
        }

        IngredientDefinition chosen =
            currentChoices[slotIndex];

        if (
            chosen == null ||
            selectedIngredients.Contains(
                chosen
            )
        )
        {
            return;
        }

        selectedIngredients.Add(
            chosen
        );

        IngredientPicked?.Invoke(
            chosen
        );

        if (
            CurrentRound >= rounds ||
            selectedIngredients.Count >= rounds
        )
        {
            CompleteDraft();
            return;
        }

        CurrentRound++;

        FillNextRoundChoices();
        ChoicesChanged?.Invoke();
    }

    public bool RerollChoice(
        int slotIndex
    )
    {
        if (
            !DraftActive ||
            RerollsRemaining <= 0 ||
            slotIndex < 0 ||
            slotIndex >= currentChoices.Count
        )
        {
            return false;
        }

        IngredientDefinition oldChoice =
            currentChoices[slotIndex];

        IngredientDefinition replacement =
            FindFreshReplacement(
                slotIndex
            );

        if (replacement == null)
        {
            replacement =
                FindFallbackReplacement(
                    slotIndex,
                    oldChoice
                );
        }

        if (replacement == null)
        {
            return false;
        }

        currentChoices[slotIndex] =
            replacement;

        seenIngredients.Add(
            replacement
        );

        RerollsRemaining--;

        ChoicesChanged?.Invoke();

        return true;
    }

    private void FillRoundOneChoices()
    {
        currentChoices.Clear();

        List<IngredientDefinition> candidates =
            GetValidStartingPool();

        Shuffle(candidates);

        for (
            int i = 0;
            i < choicesPerRound;
            i++
        )
        {
            AddShownChoice(
                candidates[i]
            );
        }
    }

    private void FillNextRoundChoices()
    {
        currentChoices.Clear();

        IngredientDefinition firstPick =
            selectedIngredients.Count > 0
                ? selectedIngredients[0]
                : null;

        IngredientDefinition synergyChoice =
            GetRandomFreshSynergyPartner(
                firstPick
            );

        if (synergyChoice != null)
        {
            AddShownChoice(
                synergyChoice
            );
        }

        List<IngredientDefinition> fresh =
            GetFreshCandidates();

        Shuffle(fresh);

        foreach (
            IngredientDefinition candidate
            in fresh
        )
        {
            if (
                currentChoices.Count >=
                choicesPerRound
            )
            {
                break;
            }

            AddShownChoice(
                candidate
            );
        }

        if (
            currentChoices.Count <
            choicesPerRound
        )
        {
            List<IngredientDefinition> fallback =
                GetFallbackCandidates(
                    -1,
                    null
                );

            Shuffle(fallback);

            foreach (
                IngredientDefinition candidate
                in fallback
            )
            {
                if (
                    currentChoices.Count >=
                    choicesPerRound
                )
                {
                    break;
                }

                AddShownChoice(
                    candidate
                );
            }
        }

        Shuffle(
            currentChoices
        );
    }

    private IngredientDefinition
        GetRandomFreshSynergyPartner(
            IngredientDefinition ingredient
        )
    {
        IReadOnlyList<IngredientDefinition>
            partners =
                runDefinition
                    .GetStartingDraftPartners(
                        ingredient
                    );

        if (partners == null)
        {
            return null;
        }

        List<IngredientDefinition> valid =
            new List<IngredientDefinition>();

        foreach (
            IngredientDefinition partner
            in partners
        )
        {
            if (
                IsFreshCandidate(
                    partner
                ) &&
                !valid.Contains(
                    partner
                )
            )
            {
                valid.Add(
                    partner
                );
            }
        }

        if (valid.Count == 0)
        {
            return null;
        }

        return valid[
            UnityEngine.Random.Range(
                0,
                valid.Count
            )
        ];
    }

    private IngredientDefinition
        FindFreshReplacement(
            int replacedSlot
        )
    {
        List<IngredientDefinition> candidates =
            GetFreshCandidates();

        RemoveOtherDisplayedChoices(
            candidates,
            replacedSlot
        );

        if (candidates.Count == 0)
        {
            return null;
        }

        return candidates[
            UnityEngine.Random.Range(
                0,
                candidates.Count
            )
        ];
    }

    private IngredientDefinition
        FindFallbackReplacement(
            int replacedSlot,
            IngredientDefinition oldChoice
        )
    {
        List<IngredientDefinition> candidates =
            GetFallbackCandidates(
                replacedSlot,
                oldChoice
            );

        if (candidates.Count == 0)
        {
            return null;
        }

        return candidates[
            UnityEngine.Random.Range(
                0,
                candidates.Count
            )
        ];
    }

    private List<IngredientDefinition>
        GetFreshCandidates()
    {
        List<IngredientDefinition> candidates =
            new List<IngredientDefinition>();

        foreach (
            IngredientDefinition ingredient
            in GetValidStartingPool()
        )
        {
            if (
                IsFreshCandidate(
                    ingredient
                )
            )
            {
                candidates.Add(
                    ingredient
                );
            }
        }

        return candidates;
    }

    private List<IngredientDefinition>
        GetFallbackCandidates(
            int replacedSlot,
            IngredientDefinition excludedChoice
        )
    {
        List<IngredientDefinition> candidates =
            GetValidStartingPool();

        candidates.RemoveAll(
            ingredient =>
                ingredient == null ||
                ingredient == excludedChoice ||
                selectedIngredients.Contains(
                    ingredient
                )
        );

        RemoveOtherDisplayedChoices(
            candidates,
            replacedSlot
        );

        return candidates;
    }

    private void RemoveOtherDisplayedChoices(
        List<IngredientDefinition> candidates,
        int replacedSlot
    )
    {
        for (
            int i = 0;
            i < currentChoices.Count;
            i++
        )
        {
            if (i == replacedSlot)
            {
                continue;
            }

            candidates.Remove(
                currentChoices[i]
            );
        }
    }

    private bool IsFreshCandidate(
        IngredientDefinition ingredient
    )
    {
        return
            ingredient != null &&
            !seenIngredients.Contains(
                ingredient
            ) &&
            !selectedIngredients.Contains(
                ingredient
            ) &&
            !currentChoices.Contains(
                ingredient
            );
    }

    private void AddShownChoice(
        IngredientDefinition ingredient
    )
    {
        if (
            ingredient == null ||
            currentChoices.Contains(
                ingredient
            ) ||
            selectedIngredients.Contains(
                ingredient
            )
        )
        {
            return;
        }

        currentChoices.Add(
            ingredient
        );

        seenIngredients.Add(
            ingredient
        );
    }

    private List<IngredientDefinition>
        GetValidStartingPool()
    {
        List<IngredientDefinition> result =
            new List<IngredientDefinition>();

        if (
            runDefinition == null ||
            runDefinition.startingDraftIngredients ==
                null
        )
        {
            return result;
        }

        foreach (
            IngredientDefinition ingredient
            in runDefinition
                .startingDraftIngredients
        )
        {
            if (
                ingredient != null &&
                !result.Contains(
                    ingredient
                )
            )
            {
                result.Add(
                    ingredient
                );
            }
        }

        return result;
    }

    private void CompleteDraft()
    {
        DraftActive = false;
        currentChoices.Clear();

        DraftCompleted?.Invoke(
            selectedIngredients
        );

        if (runManager != null)
        {
            runManager.StartRun(
                selectedIngredients,
                copiesPerPick
            );
        }
    }

    private static void Shuffle<T>(
        IList<T> list
    )
    {
        for (
            int i = list.Count - 1;
            i > 0;
            i--
        )
        {
            int j =
                UnityEngine.Random.Range(
                    0,
                    i + 1
                );

            T temporary = list[i];
            list[i] = list[j];
            list[j] = temporary;
        }
    }
}
