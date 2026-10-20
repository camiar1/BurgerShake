using UnityEngine;

public class CustomerSpeechBubbleUI :
    MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private CustomerSpawner customerSpawner;

    [SerializeField]
    private UIPopInAnimator speechBubbleAnimator;

    private void Awake()
    {
        if (customerSpawner == null)
        {
            customerSpawner =
                FindFirstObjectByType<
                    CustomerSpawner
                >();
        }
    }

    private void OnEnable()
    {
        if (customerSpawner == null)
        {
            return;
        }

        customerSpawner.CustomerArrived +=
            HandleCustomerArrived;

        customerSpawner.CustomerLeft +=
            HandleCustomerLeft;
    }

    private void OnDisable()
    {
        if (customerSpawner == null)
        {
            return;
        }

        customerSpawner.CustomerArrived -=
            HandleCustomerArrived;

        customerSpawner.CustomerLeft -=
            HandleCustomerLeft;
    }

    private void Start()
    {
        if (
            speechBubbleAnimator !=
            null
        )
        {
            speechBubbleAnimator
                .HideInstant();
        }

        if (
            customerSpawner != null &&
            customerSpawner.CustomerWaiting
        )
        {
            ShowSpeechBubble();
        }
    }

    private void HandleCustomerArrived(
        CustomerDefinition customer
    )
    {
        ShowSpeechBubble();
    }

    private void HandleCustomerLeft()
    {
        HideSpeechBubble();
    }

    public void ShowSpeechBubble()
    {
        if (
            speechBubbleAnimator ==
            null
        )
        {
            return;
        }

        speechBubbleAnimator.Show();
    }

    public void HideSpeechBubble()
    {
        if (
            speechBubbleAnimator ==
            null
        )
        {
            return;
        }

        speechBubbleAnimator.Hide();
    }
}