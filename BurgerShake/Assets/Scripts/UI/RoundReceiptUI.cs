using System.Collections;
using System.Text;
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
            descriptionText.gameObject.SetActive(
                true
            );

            descriptionText.text =
                BuildReceiptDetails(
                    customer
                );
        }

        if (goalText != null)
        {
            goalText.text =
                $"GOAL: {goalScore}";
        }
    }

    private string BuildReceiptDetails(
        CustomerDefinition customer
    )
    {
        if (customer == null)
        {
            return string.Empty;
        }

        StringBuilder builder =
            new StringBuilder();

        if (
            !string.IsNullOrWhiteSpace(
                customer.description
            )
        )
        {
            builder.AppendLine(
                customer.description.Trim()
            );

            builder.AppendLine();
        }

        builder.Append(
            "REWARD: $"
        );

        builder.AppendLine(
            Mathf.Max(
                0,
                customer.baseRewardCoins
            ).ToString()
        );

        AppendPreferences(
            builder,
            customer
        );

        AppendRestrictions(
            builder,
            customer
        );

        return builder
            .ToString()
            .TrimEnd();
    }

    private void AppendPreferences(
        StringBuilder builder,
        CustomerDefinition customer
    )
    {
        if (
            customer.preferences == null ||
            customer.preferences.Count == 0
        )
        {
            return;
        }

        builder.AppendLine();
        builder.AppendLine(
            "SPECIAL REQUEST"
        );

        foreach (
            CustomerPreference preference
            in customer.preferences
        )
        {
            if (preference == null)
            {
                continue;
            }

            string label =
                GetPreferenceLabel(
                    preference
                );

            if (
                string.IsNullOrWhiteSpace(
                    label
                )
            )
            {
                continue;
            }

            builder.Append("• ");
            builder.Append(label);

            if (preference.bonusCoins > 0)
            {
                builder.Append(
                    $" (+${preference.bonusCoins})"
                );
            }

            builder.AppendLine();
        }
    }

    private void AppendRestrictions(
        StringBuilder builder,
        CustomerDefinition customer
    )
    {
        if (
            customer.restrictions == null ||
            customer.restrictions.Count == 0
        )
        {
            return;
        }

        builder.AppendLine();
        builder.AppendLine(
            "RESTRICTIONS"
        );

        foreach (
            CustomerRestriction restriction
            in customer.restrictions
        )
        {
            if (restriction == null)
            {
                continue;
            }

            string label =
                GetRestrictionLabel(
                    restriction
                );

            if (
                string.IsNullOrWhiteSpace(
                    label
                )
            )
            {
                continue;
            }

            builder.Append("• ");
            builder.AppendLine(label);
        }
    }

    private string GetPreferenceLabel(
        CustomerPreference preference
    )
    {
        if (
            !string.IsNullOrWhiteSpace(
                preference.description
            )
        )
        {
            return preference
                .description
                .Trim();
        }

        if (
            !string.IsNullOrWhiteSpace(
                preference.preferenceName
            )
        )
        {
            return preference
                .preferenceName
                .Trim();
        }

        return "Special request";
    }

    private string GetRestrictionLabel(
        CustomerRestriction restriction
    )
    {
        if (
            !string.IsNullOrWhiteSpace(
                restriction.description
            )
        )
        {
            return restriction
                .description
                .Trim();
        }

        if (
            !string.IsNullOrWhiteSpace(
                restriction.restrictionName
            )
        )
        {
            return restriction
                .restrictionName
                .Trim();
        }

        return "Special restriction";
    }
}
