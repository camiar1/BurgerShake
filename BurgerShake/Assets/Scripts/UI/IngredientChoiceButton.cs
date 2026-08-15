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
        string text = data.basePoints + " pts";

        if (data.pointsPerTouch != 0)
        {
            text += "\n" + Signed(data.pointsPerTouch) + " pts / touch";
        }

        if (!Mathf.Approximately(data.baseMult, 0f))
        {
            text += "\n" + Signed(data.baseMult) + " Mult";
        }

        if (!Mathf.Approximately(data.multPerTouch, 0f))
        {
            text += "\n" + Signed(data.multPerTouch) + " Mult / touch";
        }

        return text;
    }

    private string Signed(float value)
    {
        return value >= 0f ? "+" + value : value.ToString();
    }
}
