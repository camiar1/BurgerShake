using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopIngredientOfferButton :
    MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private Image icon;

    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private TMP_Text copiesText;

    [SerializeField]
    private TMP_Text descriptionText;

    [SerializeField]
    private Button chooseButton;

    private IngredientDefinition ingredient;

    private Action<IngredientDefinition>
        chooseAction;

    private void Awake()
    {
        if (chooseButton != null)
        {
            chooseButton.onClick.AddListener(
                HandlePressed
            );
        }
    }

    private void OnDestroy()
    {
        if (chooseButton != null)
        {
            chooseButton.onClick.RemoveListener(
                HandlePressed
            );
        }
    }

    public void Setup(
        IngredientDefinition newIngredient,
        int ownedCopies,
        int copiesGranted,
        Action<IngredientDefinition> onChoose
    )
    {
        ingredient =
            newIngredient;

        chooseAction =
            onChoose;

        if (ingredient == null)
        {
            return;
        }

        if (icon != null)
        {
            icon.sprite =
                ingredient.sprite;

            icon.enabled =
                ingredient.sprite !=
                null;

            icon.preserveAspect =
                true;

            icon.raycastTarget =
                false;
        }

        if (nameText != null)
        {
            nameText.text =
                ingredient
                    .ingredientName;
        }

        if (copiesText != null)
        {
            if (ownedCopies > 0)
            {
                copiesText.text =
                    $"Owned ×{ownedCopies} → ×{ownedCopies + copiesGranted}";
            }
            else
            {
                copiesText.text =
                    $"NEW → ×{copiesGranted}";
            }
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                BuildDescription(
                    ingredient
                );
        }

        if (chooseButton != null)
        {
            chooseButton.interactable =
                true;
        }
    }

    private void HandlePressed()
    {
        if (ingredient == null)
        {
            return;
        }

        chooseAction?.Invoke(
            ingredient
        );
    }

    private string BuildDescription(
        IngredientDefinition definition
    )
    {
        StringBuilder builder =
            new StringBuilder();

        if (
            !string.IsNullOrWhiteSpace(
                definition.description
            )
        )
        {
            builder.AppendLine(
                definition.description
            );
        }

        foreach (
            IngredientScoringRule rule
            in definition.scoringRules
        )
        {
            if (
                rule == null ||
                string.IsNullOrWhiteSpace(
                    rule.description
                )
            )
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append(
                "• "
            );

            builder.Append(
                rule.description
            );
        }

        if (builder.Length == 0)
        {
            return
                "No scoring description.";
        }

        return builder.ToString();
    }
}