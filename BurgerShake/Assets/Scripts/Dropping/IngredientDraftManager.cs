using System.Collections.Generic;
using UnityEngine;

public class IngredientDraftManager : MonoBehaviour
{
    [Header("Ingredient Pool")]
    [SerializeField]
    private List<IngredientDefinition> ingredientPool =
        new List<IngredientDefinition>();

    [Header("Draft")]
    [SerializeField]
    [Min(1)]
    private int defaultChoiceCount = 3;

    [Header("Legacy Button UI")]
    [Tooltip(
        "Optional. Leave this empty when using the cat toss system."
    )]
    [SerializeField]
    private IngredientChoiceButton[] choiceButtons;

    [Header("Gameplay")]
    [SerializeField]
    private IngredientDropper dropper;

    [SerializeField]
    private GameplayModifiers gameplayModifiers;

    private readonly List<IngredientDefinition>
        currentChoices =
            new List<IngredientDefinition>();

    public IReadOnlyList<IngredientDefinition>
        CurrentChoices =>
            currentChoices;

    public IReadOnlyList<IngredientDefinition>
        IngredientPool =>
            ingredientPool;

    private void Awake()
    {
        if (dropper != null)
        {
            dropper.Initialize(
                this
            );
        }
    }

    private void Start()
    {
        RefreshChoices();
    }

    public void SetIngredientPool(
        IEnumerable<IngredientDefinition> ingredients
    )
    {
        ingredientPool.Clear();

        if (ingredients != null)
        {
            foreach (
                IngredientDefinition ingredient
                in ingredients
            )
            {
                if (
                    ingredient != null &&
                    !ingredientPool.Contains(
                        ingredient
                    )
                )
                {
                    ingredientPool.Add(
                        ingredient
                    );
                }
            }
        }

        RefreshChoices();
    }

    public void AddIngredientToPool(
        IngredientDefinition ingredient
    )
    {
        if (
            ingredient == null ||
            ingredientPool.Contains(
                ingredient
            )
        )
        {
            return;
        }

        ingredientPool.Add(
            ingredient
        );

        RefreshChoices();
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
        RefreshChoices();
    }

    public void RefreshChoices()
    {
        currentChoices.Clear();

        List<IngredientDefinition> available =
            new List<IngredientDefinition>();

        foreach (
            IngredientDefinition ingredient
            in ingredientPool
        )
        {
            if (
                ingredient != null &&
                ingredient.draftWeight > 0f
            )
            {
                available.Add(
                    ingredient
                );
            }
        }

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

        int count =
            Mathf.Min(
                requestedChoices,
                available.Count
            );

        for (
            int i = 0;
            i < count;
            i++
        )
        {
            IngredientDefinition chosen =
                RollWeightedChoice(
                    available
                );

            currentChoices.Add(
                chosen
            );

            available.Remove(
                chosen
            );
        }

        UpdateLegacyButtons();

        SetChoiceButtonsInteractable(
            true
        );
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
            if (choiceButtons[i] == null)
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
            if (choiceButtons[i] != null)
            {
                choiceButtons[i]
                    .gameObject
                    .SetActive(
                        false
                    );
            }
        }
    }

    private IngredientDefinition
        RollWeightedChoice(
            List<IngredientDefinition> candidates
        )
    {
        if (
            candidates == null ||
            candidates.Count == 0
        )
        {
            return null;
        }

        float totalWeight =
            0f;

        foreach (
            IngredientDefinition ingredient
            in candidates
        )
        {
            totalWeight +=
                ingredient.draftWeight;
        }

        float roll =
            Random.Range(
                0f,
                totalWeight
            );

        foreach (
            IngredientDefinition ingredient
            in candidates
        )
        {
            roll -=
                ingredient.draftWeight;

            if (roll <= 0f)
            {
                return ingredient;
            }
        }

        return candidates[
            candidates.Count - 1
        ];
    }

    private void SetChoiceButtonsInteractable(
        bool interactable
    )
    {
        if (choiceButtons == null)
        {
            return;
        }

        foreach (
            IngredientChoiceButton choiceButton
            in choiceButtons
        )
        {
            if (choiceButton != null)
            {
                choiceButton.SetInteractable(
                    interactable
                );
            }
        }
    }
}