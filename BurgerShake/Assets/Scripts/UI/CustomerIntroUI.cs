using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerIntroUI : MonoBehaviour
{
    [Header("Run")]
    [SerializeField] private RunManager runManager;

    [Header("Customer")]
    [SerializeField] private Image customerPortrait;
    [SerializeField] private TMP_Text customerNameText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Challenge")]
    [SerializeField] private TMP_Text goalText;

    [Header("Controls")]
    [SerializeField] private Button readyButton;

    private void OnEnable()
    {
        if (runManager != null)
        {
            runManager.CustomerIntroStarted +=
                ShowCustomer;
        }

        if (readyButton != null)
        {
            readyButton.onClick.AddListener(
                HandleReadyPressed
            );
        }
    }

    private void OnDisable()
    {
        if (runManager != null)
        {
            runManager.CustomerIntroStarted -=
                ShowCustomer;
        }

        if (readyButton != null)
        {
            readyButton.onClick.RemoveListener(
                HandleReadyPressed
            );
        }
    }

    private void Start()
    {
        // This handles cases where the run started
        // before this UI finished its Start().
        if (
            runManager != null &&
            runManager.CurrentCustomer != null &&
            runManager.State ==
            RunState.CustomerIntro
        )
        {
            ShowCustomer(
                runManager.CurrentCustomer,
                runManager.CurrentGoalScore
            );
        }
    }

    private void ShowCustomer(
        CustomerDefinition customer,
        int goalScore
    )
    {
        if (customer == null)
        {
            return;
        }

        if (customerNameText != null)
        {
            customerNameText.text =
                customer.customerName;
        }

        if (dialogueText != null)
        {
            dialogueText.text =
                customer.description;
        }

        if (goalText != null)
        {
            goalText.text =
                $"Goal: {goalScore}";
        }

        if (customerPortrait != null)
        {
            customerPortrait.sprite =
                customer.portrait;

            customerPortrait.enabled =
                customer.portrait != null;
        }

        if (readyButton != null)
        {
            readyButton.interactable = true;
        }
    }

    private void HandleReadyPressed()
    {
        if (runManager == null)
        {
            return;
        }

        if (readyButton != null)
        {
            readyButton.interactable = false;
        }

        runManager.BeginCurrentCustomer();
    }
}