using System.Collections;
using TMPro;
using UnityEngine;

public class RoundReceiptUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField]
    private RectTransform panel;

    [Header("Positions")]
    [SerializeField]
    private Vector2 shownAnchoredPosition;

    [SerializeField]
    private Vector2 hiddenAnchoredPosition =
        new Vector2(
            0f,
            400f
        );

    [Header("Customer Text")]
    [SerializeField]
    private TMP_Text customerNameText;

    [SerializeField]
    private TMP_Text descriptionText;

    [SerializeField]
    private TMP_Text goalText;

    [Header("Animation")]
    [SerializeField]
    private float slideDuration =
        0.45f;

    [SerializeField]
    private AnimationCurve slideCurve =
        AnimationCurve.EaseInOut(
            0f,
            0f,
            1f,
            1f
        );

    private void Awake()
    {
        if (panel == null)
        {
            panel =
                GetComponent<
                    RectTransform
                >();
        }

        HideInstant();
    }

    public void HideInstant()
    {
        if (panel == null)
        {
            return;
        }

        panel.anchoredPosition =
            hiddenAnchoredPosition;
    }

    public IEnumerator ShowRoutine(
        CustomerDefinition customer,
        int goalScore
    )
    {
        SetCustomerInfo(
            customer,
            goalScore
        );

        if (panel == null)
        {
            yield break;
        }

        panel.anchoredPosition =
            hiddenAnchoredPosition;

        float duration =
            Mathf.Max(
                0.01f,
                slideDuration
            );

        float elapsed =
            0f;

        while (elapsed < duration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float normalized =
                Mathf.Clamp01(
                    elapsed /
                    duration
                );

            float curved =
                slideCurve != null
                    ? slideCurve
                        .Evaluate(
                            normalized
                        )
                    : normalized;

            panel.anchoredPosition =
                Vector2.LerpUnclamped(
                    hiddenAnchoredPosition,
                    shownAnchoredPosition,
                    curved
                );

            yield return null;
        }

        panel.anchoredPosition =
            shownAnchoredPosition;
    }

    private void SetCustomerInfo(
        CustomerDefinition customer,
        int goalScore
    )
    {
        if (customerNameText != null)
        {
            customerNameText.text =
                customer != null
                    ? customer.customerName
                    : "Customer";
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                customer != null
                    ? customer.description
                    : "";
        }

        if (goalText != null)
        {
            goalText.text =
                $"GOAL: {goalScore}";
        }
    }
}