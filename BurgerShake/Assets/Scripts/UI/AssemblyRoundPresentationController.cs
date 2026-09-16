using System.Collections;
using UnityEngine;

public class AssemblyRoundPresentationController :
    MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField]
    private CustomerChallengeController
        challengeController;

    [SerializeField]
    private RoundReceiptUI receiptUI;

    [SerializeField]
    private CatTossDraftController
        tossController;

    [Header("Timing")]
    [SerializeField]
    private float delayAfterReceipt =
        0.15f;

    private Coroutine introRoutine;

    private void OnEnable()
    {
        if (challengeController != null)
        {
            challengeController
                .ChallengeStarted +=
                    HandleChallengeStarted;
        }
    }

    private void OnDisable()
    {
        if (challengeController != null)
        {
            challengeController
                .ChallengeStarted -=
                    HandleChallengeStarted;
        }

        if (introRoutine != null)
        {
            StopCoroutine(
                introRoutine
            );

            introRoutine =
                null;
        }
    }

    private void HandleChallengeStarted(
        CustomerDefinition customer,
        int goalScore
    )
    {
        if (introRoutine != null)
        {
            StopCoroutine(
                introRoutine
            );
        }

        introRoutine =
            StartCoroutine(
                RoundIntroRoutine(
                    customer,
                    goalScore
                )
            );
    }

    private IEnumerator RoundIntroRoutine(
        CustomerDefinition customer,
        int goalScore
    )
    {
        if (receiptUI != null)
        {
            yield return
                receiptUI.ShowRoutine(
                    customer,
                    goalScore
                );
        }

        if (delayAfterReceipt > 0f)
        {
            yield return
                new WaitForSecondsRealtime(
                    delayAfterReceipt
                );
        }

        if (tossController != null)
        {
            tossController.BeginRound();
        }

        introRoutine =
            null;
    }
}