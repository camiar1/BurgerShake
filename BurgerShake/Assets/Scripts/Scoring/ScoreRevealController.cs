using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreRevealController :
    MonoBehaviour
{
    [Header("Managers")]
    [SerializeField]
    private ScoreManager scoreManager;

    [SerializeField]
    private ScoreUI scoreUI;

    [Header("Ingredient Popup")]
    [SerializeField]
    private TextMeshPro ingredientPopupText;

    [SerializeField]
    private Vector3 popupOffset =
        new Vector3(
            0f,
            0.8f,
            0f
        );

    [SerializeField]
    private int popupSortingOrder = 100;

    [Header("Popup Animation")]
    [SerializeField]
    private float popupRiseDistance = 0.25f;

    [SerializeField]
    private float popupDuration = 0.6f;

    [SerializeField]
    private float popupStartScale = 0.6f;

    [SerializeField]
    private float popupPeakScale = 1.2f;

    [Header("Highlight")]
    [SerializeField]
    private Color highlightColor =
        new Color(
            1f,
            0.85f,
            0.1f,
            1f
        );

    [SerializeField]
    private float highlightWidth = 0.04f;

    [Header("Timing")]
    [SerializeField]
    private float initialDelay = 0.6f;

    [SerializeField]
    private float betweenStepsDelay = 0.18f;

    [SerializeField]
    private float finalHold = 0.8f;

    private Coroutine popupRoutine;

    private void Awake()
    {
        if (scoreManager == null)
        {
            scoreManager =
                FindFirstObjectByType<ScoreManager>();
        }

        if (scoreUI == null)
        {
            scoreUI =
                FindFirstObjectByType<ScoreUI>();
        }

        HidePopupImmediately();
    }

    public IEnumerator PlayReveal()
    {
        if (scoreManager == null)
        {
            yield break;
        }

        scoreManager.CalculateFinalScore();

        if (scoreUI != null)
        {
            scoreUI.BeginScoreReveal(
                scoreManager.StartingMultValue
            );
        }

        if (initialDelay > 0f)
        {
            yield return
                new WaitForSecondsRealtime(
                    initialDelay
                );
        }

        foreach (
            IngredientScoreStep step
            in scoreManager.LastBreakdown
        )
        {
            if (
                step == null ||
                step.ingredient == null
            )
            {
                continue;
            }

            bool pointsChanged =
                step.contribution.points != 0;

            bool multChanged =
                !Mathf.Approximately(
                    step.contribution.mult,
                    0f
                );

            if (
                !pointsChanged &&
                !multChanged
            )
            {
                continue;
            }

            IngredientHighlight highlight =
                GetHighlight(
                    step.ingredient
                );

            if (highlight != null)
            {
                highlight.Show(
                    highlightColor,
                    highlightWidth
                );
            }

            yield return PlayPopup(
                step
            );

            if (
                pointsChanged &&
                scoreUI != null
            )
            {
                yield return
                    scoreUI.AnimatePoints(
                        step.pointsBefore,
                        step.pointsAfter
                    );
            }

            if (
                multChanged &&
                scoreUI != null
            )
            {
                yield return
                    scoreUI.AnimateMult(
                        step.multBefore,
                        step.multAfter
                    );
            }

            if (
                scoreUI != null &&
                step.totalBefore !=
                step.totalAfter
            )
            {
                yield return
                    scoreUI.AnimateTotal(
                        step.totalBefore,
                        step.totalAfter
                    );
            }

            if (highlight != null)
            {
                highlight.Hide();
            }

            HidePopupImmediately();

            if (betweenStepsDelay > 0f)
            {
                yield return
                    new WaitForSecondsRealtime(
                        betweenStepsDelay
                    );
            }
        }

        HidePopupImmediately();

        if (finalHold > 0f)
        {
            yield return
                new WaitForSecondsRealtime(
                    finalHold
                );
        }
    }

    private IEnumerator PlayPopup(
        IngredientScoreStep step
    )
    {
        if (
            ingredientPopupText == null ||
            step == null ||
            step.ingredient == null
        )
        {
            yield break;
        }

        if (popupRoutine != null)
        {
            StopCoroutine(
                popupRoutine
            );

            popupRoutine = null;
        }

        ingredientPopupText.text =
            BuildContributionText(
                step
            );

        Vector3 startPosition =
            step.ingredient
                .transform
                .position +
            popupOffset;

        Vector3 endPosition =
            startPosition +
            Vector3.up *
            popupRiseDistance;

        ingredientPopupText
            .transform
            .position =
                startPosition;

        ingredientPopupText
            .transform
            .localScale =
                Vector3.one *
                popupStartScale;

        Color originalColor =
            ingredientPopupText.color;

        Color visibleColor =
            originalColor;

        visibleColor.a = 1f;

        ingredientPopupText.color =
            visibleColor;

        SpriteRenderer spriteRenderer =
            step.ingredient
                .GetComponentInChildren<
                    SpriteRenderer
                >();

        if (spriteRenderer != null)
        {
            ingredientPopupText
                .renderer
                .sortingLayerID =
                    spriteRenderer
                        .sortingLayerID;

            ingredientPopupText
                .renderer
                .sortingOrder =
                    spriteRenderer
                        .sortingOrder +
                    popupSortingOrder;
        }

        ingredientPopupText
            .gameObject
            .SetActive(true);

        float duration =
            Mathf.Max(
                0.01f,
                popupDuration
            );

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            ingredientPopupText
                .transform
                .position =
                    Vector3.Lerp(
                        startPosition,
                        endPosition,
                        t
                    );

            float scale;

            if (t < 0.25f)
            {
                scale =
                    Mathf.Lerp(
                        popupStartScale,
                        popupPeakScale,
                        t / 0.25f
                    );
            }
            else
            {
                scale =
                    Mathf.Lerp(
                        popupPeakScale,
                        1f,
                        (t - 0.25f) /
                        0.75f
                    );
            }

            ingredientPopupText
                .transform
                .localScale =
                    Vector3.one *
                    scale;

            yield return null;
        }

        ingredientPopupText
            .transform
            .position =
                endPosition;

        ingredientPopupText
            .transform
            .localScale =
                Vector3.one;
    }

    private IngredientHighlight
        GetHighlight(
            Ingredient ingredient
        )
    {
        IngredientHighlight highlight =
            ingredient.GetComponent<
                IngredientHighlight
            >();

        if (highlight == null)
        {
            highlight =
                ingredient.gameObject
                    .AddComponent<
                        IngredientHighlight
                    >();
        }

        return highlight;
    }

    private string BuildContributionText(
        IngredientScoreStep step
    )
    {
        int points =
            step.contribution.points;

        float mult =
            step.contribution.mult;

        bool hasPoints =
            points != 0;

        bool hasMult =
            !Mathf.Approximately(
                mult,
                0f
            );

        if (
            hasPoints &&
            hasMult
        )
        {
            return
                $"{Signed(points)} PTS\n" +
                $"{Signed(mult)} MULT";
        }

        if (hasPoints)
        {
            return
                $"{Signed(points)} PTS";
        }

        if (hasMult)
        {
            return
                $"{Signed(mult)} MULT";
        }

        return "";
    }

    private string Signed(
        int value
    )
    {
        return value >= 0
            ? $"+{value}"
            : value.ToString();
    }

    private string Signed(
        float value
    )
    {
        return value >= 0f
            ? $"+{value:0.##}"
            : value.ToString(
                "0.##"
            );
    }

    private void HidePopupImmediately()
    {
        if (ingredientPopupText != null)
        {
            ingredientPopupText
                .gameObject
                .SetActive(false);
        }
    }
}