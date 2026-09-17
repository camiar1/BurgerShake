using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartingIngredientDraftUI :
    MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private StartingIngredientDraftController
        draftController;

    [SerializeField]
    private GameObject contentRoot;

    [Header("Header")]
    [SerializeField]
    private TMP_Text roundText;

    [SerializeField]
    private TMP_Text rerollText;

    [Header("Choice Cards")]
    [SerializeField]
    private Button[] choiceButtons =
        new Button[3];

    [SerializeField]
    private Image[] ingredientImages =
        new Image[3];

    [SerializeField]
    private TMP_Text[] ingredientNameTexts =
        new TMP_Text[3];

    [SerializeField]
    private TMP_Text[] ingredientDescriptionTexts =
        new TMP_Text[3];

    [SerializeField]
    private Button[] rerollButtons =
        new Button[3];

    private void Awake()
    {
        if (draftController == null)
        {
            draftController =
                FindFirstObjectByType<
                    StartingIngredientDraftController
                >();
        }
    }

    private void OnEnable()
    {
        Subscribe();
        RegisterButtonListeners();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
        RemoveButtonListeners();
    }

    private void Subscribe()
    {
        if (draftController == null)
        {
            return;
        }

        draftController.DraftStarted +=
            HandleDraftStarted;

        draftController.ChoicesChanged +=
            Refresh;

        draftController.DraftCompleted +=
            HandleDraftCompleted;
    }

    private void Unsubscribe()
    {
        if (draftController == null)
        {
            return;
        }

        draftController.DraftStarted -=
            HandleDraftStarted;

        draftController.ChoicesChanged -=
            Refresh;

        draftController.DraftCompleted -=
            HandleDraftCompleted;
    }

    private void RegisterButtonListeners()
    {
        if (
            choiceButtons != null &&
            choiceButtons.Length > 0 &&
            choiceButtons[0] != null
        )
        {
            choiceButtons[0].onClick.AddListener(
                ChooseSlot0
            );
        }

        if (
            choiceButtons != null &&
            choiceButtons.Length > 1 &&
            choiceButtons[1] != null
        )
        {
            choiceButtons[1].onClick.AddListener(
                ChooseSlot1
            );
        }

        if (
            choiceButtons != null &&
            choiceButtons.Length > 2 &&
            choiceButtons[2] != null
        )
        {
            choiceButtons[2].onClick.AddListener(
                ChooseSlot2
            );
        }

        if (
            rerollButtons != null &&
            rerollButtons.Length > 0 &&
            rerollButtons[0] != null
        )
        {
            rerollButtons[0].onClick.AddListener(
                RerollSlot0
            );
        }

        if (
            rerollButtons != null &&
            rerollButtons.Length > 1 &&
            rerollButtons[1] != null
        )
        {
            rerollButtons[1].onClick.AddListener(
                RerollSlot1
            );
        }

        if (
            rerollButtons != null &&
            rerollButtons.Length > 2 &&
            rerollButtons[2] != null
        )
        {
            rerollButtons[2].onClick.AddListener(
                RerollSlot2
            );
        }
    }

    private void RemoveButtonListeners()
    {
        if (
            choiceButtons != null &&
            choiceButtons.Length > 0 &&
            choiceButtons[0] != null
        )
        {
            choiceButtons[0].onClick.RemoveListener(
                ChooseSlot0
            );
        }

        if (
            choiceButtons != null &&
            choiceButtons.Length > 1 &&
            choiceButtons[1] != null
        )
        {
            choiceButtons[1].onClick.RemoveListener(
                ChooseSlot1
            );
        }

        if (
            choiceButtons != null &&
            choiceButtons.Length > 2 &&
            choiceButtons[2] != null
        )
        {
            choiceButtons[2].onClick.RemoveListener(
                ChooseSlot2
            );
        }

        if (
            rerollButtons != null &&
            rerollButtons.Length > 0 &&
            rerollButtons[0] != null
        )
        {
            rerollButtons[0].onClick.RemoveListener(
                RerollSlot0
            );
        }

        if (
            rerollButtons != null &&
            rerollButtons.Length > 1 &&
            rerollButtons[1] != null
        )
        {
            rerollButtons[1].onClick.RemoveListener(
                RerollSlot1
            );
        }

        if (
            rerollButtons != null &&
            rerollButtons.Length > 2 &&
            rerollButtons[2] != null
        )
        {
            rerollButtons[2].onClick.RemoveListener(
                RerollSlot2
            );
        }
    }

    private void HandleDraftStarted()
    {
        SetContentVisible(
            true
        );

        Refresh();
    }

    private void HandleDraftCompleted(
        System.Collections.Generic
            .IReadOnlyList<IngredientDefinition>
            ingredients
    )
    {
        SetContentVisible(
            false
        );
    }

    private void Refresh()
    {
        if (draftController == null)
        {
            SetContentVisible(
                false
            );

            return;
        }

        SetContentVisible(
            draftController.DraftActive
        );

        if (!draftController.DraftActive)
        {
            return;
        }

        if (roundText != null)
        {
            roundText.text =
                $"PICK {draftController.CurrentRound} OF {draftController.RoundCount}";
        }

        if (rerollText != null)
        {
            rerollText.text =
                draftController.RerollsRemaining > 0
                    ? $"REROLLS: {draftController.RerollsRemaining}"
                    : "NO REROLLS LEFT";
        }

        int slotCount =
            GetSlotCount();

        for (
            int i = 0;
            i < slotCount;
            i++
        )
        {
            IngredientDefinition ingredient =
                i < draftController
                    .CurrentChoices.Count
                    ? draftController
                        .CurrentChoices[i]
                    : null;

            RefreshSlot(
                i,
                ingredient
            );
        }
    }

    private void RefreshSlot(
        int index,
        IngredientDefinition ingredient
    )
    {
        bool hasIngredient =
            ingredient != null;

        if (
            choiceButtons != null &&
            index < choiceButtons.Length &&
            choiceButtons[index] != null
        )
        {
            choiceButtons[index]
                .gameObject
                .SetActive(
                    hasIngredient
                );

            choiceButtons[index]
                .interactable =
                    hasIngredient;

            IngredientTooltipTrigger tooltipTrigger =
                choiceButtons[index]
                    .GetComponent<IngredientTooltipTrigger>();

            if (tooltipTrigger == null)
                tooltipTrigger = choiceButtons[index]
                    .gameObject.AddComponent<IngredientTooltipTrigger>();

            tooltipTrigger.Setup(ingredient);
        }

        if (
            ingredientImages != null &&
            index < ingredientImages.Length &&
            ingredientImages[index] != null
        )
        {
            ingredientImages[index].sprite =
                hasIngredient
                    ? ingredient.sprite
                    : null;

            ingredientImages[index].enabled =
                hasIngredient &&
                ingredient.sprite != null;
        }

        if (
            ingredientNameTexts != null &&
            index < ingredientNameTexts.Length &&
            ingredientNameTexts[index] != null
        )
        {
            ingredientNameTexts[index].text =
                hasIngredient
                    ? ingredient.ingredientName
                    : string.Empty;
        }

        if (
            ingredientDescriptionTexts != null &&
            index < ingredientDescriptionTexts.Length &&
            ingredientDescriptionTexts[index] != null
        )
        {
            ingredientDescriptionTexts[index].text =
                hasIngredient
                    ? ingredient.description
                    : string.Empty;
        }

        if (
            rerollButtons != null &&
            index < rerollButtons.Length &&
            rerollButtons[index] != null
        )
        {
            rerollButtons[index]
                .gameObject
                .SetActive(
                    hasIngredient
                );

            rerollButtons[index]
                .interactable =
                    hasIngredient &&
                    draftController
                        .RerollsRemaining > 0;
        }
    }

    private int GetSlotCount()
    {
        int count = 0;

        if (choiceButtons != null)
        {
            count = Mathf.Max(
                count,
                choiceButtons.Length
            );
        }

        if (ingredientImages != null)
        {
            count = Mathf.Max(
                count,
                ingredientImages.Length
            );
        }

        if (ingredientNameTexts != null)
        {
            count = Mathf.Max(
                count,
                ingredientNameTexts.Length
            );
        }

        if (ingredientDescriptionTexts != null)
        {
            count = Mathf.Max(
                count,
                ingredientDescriptionTexts.Length
            );
        }

        if (rerollButtons != null)
        {
            count = Mathf.Max(
                count,
                rerollButtons.Length
            );
        }

        return count;
    }

    private void SetContentVisible(
        bool visible
    )
    {
        if (contentRoot != null)
        {
            contentRoot.SetActive(
                visible
            );
        }
    }

    private void ChooseSlot0()
    {
        draftController?.Choose(0);
    }

    private void ChooseSlot1()
    {
        draftController?.Choose(1);
    }

    private void ChooseSlot2()
    {
        draftController?.Choose(2);
    }

    private void RerollSlot0()
    {
        draftController?.RerollChoice(0);
    }

    private void RerollSlot1()
    {
        draftController?.RerollChoice(1);
    }

    private void RerollSlot2()
    {
        draftController?.RerollChoice(2);
    }
}
