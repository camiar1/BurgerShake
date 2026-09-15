using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientChoiceButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text scoringText;
    [SerializeField] private Button button;

    private IngredientDefinition ingredient;
    private IngredientDraftManager draftManager;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
    }

    public void Setup(IngredientDefinition newIngredient, IngredientDraftManager manager)
    {
        ingredient = newIngredient;
        draftManager = manager;

        if (ingredient == null)
        {
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = ingredient.sprite;
            iconImage.enabled = ingredient.sprite != null;
            iconImage.preserveAspect = true;
        }

        if (nameText != null)
        {
            nameText.text = ingredient.ingredientName;
        }

        if (scoringText != null)
        {
            scoringText.text = BuildScoringText(ingredient);
        }

        if (button != null)
        {
            button.onClick.RemoveListener(Choose);
            button.onClick.AddListener(Choose);
        }
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    private void Choose()
    {
        draftManager?.SelectIngredient(ingredient);
    }

    private string BuildScoringText(IngredientDefinition data)
    {
        if (data.scoringRules == null || data.scoringRules.Count == 0)
        {
            return "No scoring rule";
        }

        StringBuilder builder = new StringBuilder();

        foreach (IngredientScoringRule rule in data.scoringRules)
        {
            if (rule == null)
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append(string.IsNullOrWhiteSpace(rule.description)
                ? BuildFallbackRuleText(rule)
                : rule.description);
        }

        return builder.ToString();
    }

    private string BuildFallbackRuleText(IngredientScoringRule rule)
    {
        string rewardText = rule.reward == ScoringReward.Points ? " pts" : " Mult";
        string prefix = rule.amount >= 0f ? "+" : string.Empty;

        switch (rule.target)
        {
            case ScoringTarget.Self:
                return prefix + rule.amount + rewardText;
            case ScoringTarget.TouchingAny:
                return prefix + rule.amount + rewardText + " / touch";
            case ScoringTarget.TouchingTag:
                return prefix + rule.amount + rewardText + " / touching " + rule.requiredTag;
            case ScoringTarget.TouchingIngredient:
                string ingredientName = rule.requiredIngredient != null
                    ? rule.requiredIngredient.ingredientName
                    : "ingredient";
                return prefix + rule.amount + rewardText + " / touching " + ingredientName;
            default:
                return prefix + rule.amount + rewardText;
        }
    }
}
