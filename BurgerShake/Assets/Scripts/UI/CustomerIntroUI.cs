using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerIntroUI : MonoBehaviour
{
    [Header("Run")]
    [SerializeField] private RunManager runManager;

    [Header("Content")]
    [SerializeField] private GameObject introContent;

    [Header("Text")]
    [SerializeField] private TMP_Text customerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text goalText;

    [Header("Controls")]
    [SerializeField] private Button readyButton;

    [Header("Fallback Dialogue")]
    [TextArea]
    [SerializeField]
    private string defaultDialogue =
        "I've heard about these burger shakes. I'd love to give one a try!";

    private void Awake()
    {
        if (runManager == null)
        {
            runManager =
                FindFirstObjectByType<RunManager>();
        }
    }

    private void OnEnable()
    {
        if (runManager != null)
        {
            runManager.CustomerIntroStarted +=
                ShowCustomer;

            runManager.StateChanged +=
                HandleRunStateChanged;
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

            runManager.StateChanged -=
                HandleRunStateChanged;
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
        SetContentVisible(false);

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

        SetContentVisible(true);

        if (customerNameText != null)
        {
            customerNameText.text =
                customer.customerName;
        }

        if (dialogueText != null)
        {
            if (
                string.IsNullOrWhiteSpace(
                    customer.description
                )
            )
            {
                dialogueText.text =
                    defaultDialogue;
            }
            else
            {
                dialogueText.text =
                    customer.description;
            }
        }

        if (goalText != null)
        {
            goalText.text =
                $"Goal: {goalScore}";
        }

        if (readyButton != null)
        {
            readyButton.interactable =
                true;
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
            readyButton.interactable =
                false;
        }

        runManager.BeginCurrentCustomer();
    }

    private void HandleRunStateChanged(
        RunState state
    )
    {
        if (
            state != RunState.CustomerIntro
        )
        {
            SetContentVisible(false);
        }
    }

    private void SetContentVisible(
        bool visible
    )
    {
        if (introContent != null)
        {
            introContent.SetActive(
                visible
            );
        }
    }
}