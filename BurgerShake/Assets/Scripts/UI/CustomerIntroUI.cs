using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerIntroUI : MonoBehaviour
{
    [Header("Run")]
    [SerializeField]
    private RunManager runManager;

    [SerializeField]
    private ViewController viewController;

    [Header("Content")]
    [SerializeField]
    private GameObject introContent;

    [SerializeField]
    private UISlideAnimator
        windowCustomerSlideAnimator;

    [Header("Text")]
    [SerializeField]
    private TMP_Text customerNameText;

    [SerializeField]
    private TMP_Text dialogueText;

    [SerializeField]
    private TMP_Text goalText;

    [Header("Controls")]
    [SerializeField]
    private Button readyButton;

    [Header("Fallback Dialogue")]
    [TextArea]
    [SerializeField]
    private string defaultDialogue =
        "I've heard about these burger shakes. " +
        "I'd love to give one a try!";

    private Coroutine
        windowSlideRoutine;

    private void Awake()
    {
        if (runManager == null)
        {
            runManager =
                FindFirstObjectByType<
                    RunManager
                >();
        }

        if (viewController == null)
        {
            viewController =
                FindFirstObjectByType<
                    ViewController
                >();
        }

        if (
            windowCustomerSlideAnimator ==
                null &&
            introContent != null
        )
        {
            windowCustomerSlideAnimator =
                introContent
                    .GetComponentInChildren<
                        UISlideAnimator
                    >(
                        true
                    );
        }
    }

    private void OnEnable()
    {
        if (runManager != null)
        {
            runManager.CustomerIntroStarted +=
                HandleCustomerIntroStarted;

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
                HandleCustomerIntroStarted;

            runManager.StateChanged -=
                HandleRunStateChanged;
        }

        if (readyButton != null)
        {
            readyButton.onClick.RemoveListener(
                HandleReadyPressed
            );
        }

        StopWindowSlideRoutine();
    }

    private void Start()
    {
        if (
            runManager != null &&
            runManager.State ==
                RunState.CustomerIntro
        )
        {
            BeginIntroPresentation();
        }
        else
        {
            SetContentVisible(
                false
            );
        }
    }

    private void HandleRunStateChanged(
        RunState state
    )
    {
        if (
            state ==
            RunState.CustomerIntro
        )
        {
            BeginIntroPresentation();

            return;
        }

        StopWindowSlideRoutine();

        SetContentVisible(
            false
        );
    }

    private void BeginIntroPresentation()
    {
        StopWindowSlideRoutine();

        SetContentVisible(
            true
        );

        PopulateCurrentCustomer();

        SetReadyInteractable(
            false
        );

        if (
            windowCustomerSlideAnimator !=
            null
        )
        {
            windowCustomerSlideAnimator
                .HideInstant();
        }

        windowSlideRoutine =
            StartCoroutine(
                WaitForCustomerWindowAndSlide()
            );
    }

    private IEnumerator
        WaitForCustomerWindowAndSlide()
    {
        if (viewController != null)
        {
            while (
                viewController.IsSliding ||
                viewController.CurrentView !=
                    ViewController
                        .FoodTruckView
                        .CustomerWindow
            )
            {
                yield return null;
            }
        }

        // Wait one frame so the HUD controller
        // has time to make WindowUI visible.
        yield return null;

        if (
            windowCustomerSlideAnimator !=
            null
        )
        {
            windowCustomerSlideAnimator
                .Show();
        }

        windowSlideRoutine =
            null;
    }

    private void
        HandleCustomerIntroStarted(
            CustomerDefinition customer,
            int goalScore
        )
    {
        PopulateCustomer(
            customer,
            goalScore
        );

        SetReadyInteractable(
            true
        );
    }

    private void PopulateCurrentCustomer()
    {
        if (
            runManager == null ||
            runManager.CurrentCustomer ==
                null
        )
        {
            ClearCustomerText();

            return;
        }

        PopulateCustomer(
            runManager.CurrentCustomer,
            runManager.CurrentGoalScore
        );
    }

    private void PopulateCustomer(
        CustomerDefinition customer,
        int goalScore
    )
    {
        if (customer == null)
        {
            ClearCustomerText();

            return;
        }

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
    }

    private void ClearCustomerText()
    {
        if (customerNameText != null)
        {
            customerNameText.text =
                "";
        }

        if (dialogueText != null)
        {
            dialogueText.text =
                "";
        }

        if (goalText != null)
        {
            goalText.text =
                "";
        }
    }

    private void HandleReadyPressed()
    {
        if (runManager == null)
        {
            return;
        }

        SetReadyInteractable(
            false
        );

        runManager.BeginCurrentCustomer();
    }

    private void SetReadyInteractable(
        bool interactable
    )
    {
        if (readyButton != null)
        {
            readyButton.interactable =
                interactable;
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

    private void StopWindowSlideRoutine()
    {
        if (windowSlideRoutine == null)
        {
            return;
        }

        StopCoroutine(
            windowSlideRoutine
        );

        windowSlideRoutine =
            null;
    }
}