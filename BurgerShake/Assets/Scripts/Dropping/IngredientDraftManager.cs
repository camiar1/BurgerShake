using System.Collections.Generic;
using UnityEngine;

public class IngredientDraftManager : MonoBehaviour
{
    [SerializeField] private List<IngredientDefinition> ingredientPool = new List<IngredientDefinition>();
    [SerializeField] private IngredientChoiceButton[] choiceButtons;
    [SerializeField] private IngredientDropper dropper;
    [SerializeField] private GameplayModifiers gameplayModifiers;

    private readonly List<IngredientDefinition> currentChoices = new List<IngredientDefinition>();

    public IReadOnlyList<IngredientDefinition> CurrentChoices => currentChoices;
    public IReadOnlyList<IngredientDefinition> IngredientPool => ingredientPool;

    private void Awake()
    {
        if (dropper != null)
        {
            dropper.Initialize(this);
        }
    }

    private void Start()
    {
        RefreshChoices();
    }

    public void SetIngredientPool(IEnumerable<IngredientDefinition> ingredients)
    {
        ingredientPool.Clear();

        if (ingredients != null)
        {
            foreach (IngredientDefinition ingredient in ingredients)
            {
                if (ingredient != null && !ingredientPool.Contains(ingredient))
                {
                    ingredientPool.Add(ingredient);
                }
            }
        }

        RefreshChoices();
    }

    public void AddIngredientToPool(IngredientDefinition ingredient)
    {
        if (ingredient != null && !ingredientPool.Contains(ingredient))
        {
            ingredientPool.Add(ingredient);
            RefreshChoices();
        }
    }

    public void SelectIngredient(IngredientDefinition ingredient)
    {
        if (ingredient == null || !currentChoices.Contains(ingredient) || dropper == null)
        {
            return;
        }

        dropper.SetIngredient(ingredient);
        SetChoiceButtonsInteractable(false);
    }

    public void IngredientWasDropped()
    {
        RefreshChoices();
    }

    public void RefreshChoices()
    {
        currentChoices.Clear();

        List<IngredientDefinition> available = new List<IngredientDefinition>();
        foreach (IngredientDefinition ingredient in ingredientPool)
        {
            if (ingredient != null && ingredient.draftWeight > 0f)
            {
                available.Add(ingredient);
            }
        }

        int requestedChoices = gameplayModifiers != null
            ? gameplayModifiers.DraftChoiceCount
            : choiceButtons.Length;

        int count = Mathf.Min(requestedChoices, choiceButtons.Length, available.Count);

        for (int i = 0; i < count; i++)
        {
            IngredientDefinition chosen = RollWeightedChoice(available);
            currentChoices.Add(chosen);
            available.Remove(chosen);
            choiceButtons[i].Setup(chosen, this);
            choiceButtons[i].gameObject.SetActive(true);
        }

        for (int i = count; i < choiceButtons.Length; i++)
        {
            choiceButtons[i].gameObject.SetActive(false);
        }

        SetChoiceButtonsInteractable(true);
    }

    private IngredientDefinition RollWeightedChoice(List<IngredientDefinition> candidates)
    {
        float totalWeight = 0f;

        foreach (IngredientDefinition ingredient in candidates)
        {
            totalWeight += ingredient.draftWeight;
        }

        float roll = Random.Range(0f, totalWeight);

        foreach (IngredientDefinition ingredient in candidates)
        {
            roll -= ingredient.draftWeight;
            if (roll <= 0f)
            {
                return ingredient;
            }
        }

        return candidates[candidates.Count - 1];
    }

    private void SetChoiceButtonsInteractable(bool interactable)
    {
        foreach (IngredientChoiceButton choiceButton in choiceButtons)
        {
            if (choiceButton != null)
            {
                choiceButton.SetInteractable(interactable);
            }
        }
    }
}
