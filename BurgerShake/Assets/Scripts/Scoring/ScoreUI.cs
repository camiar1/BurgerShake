using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField]
    private RunManager runManager;

    [Header("Score Text")]
    [SerializeField]
    private TMP_Text pointsText;

    [SerializeField]
    private TMP_Text multText;

    [SerializeField]
    private TMP_Text totalScoreText;

    [SerializeField]
    private TMP_Text goalText;

    [Header("Number Animation")]
    [SerializeField]
    private float countDuration = 0.25f;

    [Header("Pop Animation")]
    [SerializeField]
    private float popScale = 1.2f;

    [SerializeField]
    private float popDuration = 0.18f;

    [SerializeField]
    private AnimationCurve popCurve =
        AnimationCurve.EaseInOut(
            0f,
            0f,
            1f,
            1f
        );

    private int currentGoalScore;

    private Vector3 pointsBaseScale;
    private Vector3 multBaseScale;
    private Vector3 totalBaseScale;
    private Vector3 goalBaseScale;

    private void Awake()
    {
        if (runManager == null)
        {
            runManager =
                FindFirstObjectByType<RunManager>();
        }

        if (pointsText != null)
        {
            pointsBaseScale =
                pointsText.rectTransform.localScale;
        }

        if (multText != null)
        {
            multBaseScale =
                multText.rectTransform.localScale;
        }

        if (totalScoreText != null)
        {
            totalBaseScale =
                totalScoreText.rectTransform.localScale;
        }

        if (goalText != null)
        {
            goalBaseScale =
                goalText.rectTransform.localScale;
        }
    }

    private void OnEnable()
    {
        if (runManager != null)
        {
            runManager.CustomerIntroStarted +=
                HandleCustomerIntroStarted;

            runManager.StateChanged +=
                HandleStateChanged;
        }
    }

    private void OnDisable()
    {
        if (runManager != null)
        {
            runManager.CustomerIntroStarted -=
                HandleCustomerIntroStarted;

            runManager.StateChanged -=
                HandleStateChanged;
        }

        ResetScales();
    }

    private void Start()
    {
        if (runManager != null)
        {
            currentGoalScore =
                runManager.CurrentGoalScore;
        }

        UpdateGoal();

        PrepareForAssembly();
    }

    private void HandleCustomerIntroStarted(
        CustomerDefinition customer,
        int goalScore
    )
    {
        currentGoalScore =
            goalScore;

        UpdateGoal();
    }

    private void HandleStateChanged(
        RunState state
    )
    {
        if (state == RunState.Assembly)
        {
            PrepareForAssembly();
        }
    }

    public void PrepareForAssembly()
    {
        if (pointsText != null)
        {
            pointsText.text =
                "POINTS\n?";
        }

        if (multText != null)
        {
            multText.text =
                "MULT\n?";
        }

        if (totalScoreText != null)
        {
            totalScoreText.text =
                "SCORE\n?";
        }

        UpdateGoal();

        ResetScales();
    }

    public void BeginScoreReveal(
        float startingMult
    )
    {
        if (pointsText != null)
        {
            pointsText.text =
                "POINTS\n0";
        }

        if (multText != null)
        {
            multText.text =
                $"MULT\n×{startingMult:0.##}";
        }

        if (totalScoreText != null)
        {
            totalScoreText.text =
                "SCORE\n0";
        }

        ResetScales();
    }

    public IEnumerator AnimatePoints(
        int from,
        int to
    )
    {
        if (pointsText == null)
        {
            yield break;
        }

        yield return AnimateInteger(
            pointsText,
            "POINTS",
            from,
            to
        );

        yield return PopRoutine(
            pointsText.rectTransform,
            pointsBaseScale
        );
    }

    public IEnumerator AnimateMult(
        float from,
        float to
    )
    {
        if (multText == null)
        {
            yield break;
        }

        float duration =
            Mathf.Max(
                0.01f,
                countDuration
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

            float value =
                Mathf.Lerp(
                    from,
                    to,
                    t
                );

            multText.text =
                $"MULT\n×{value:0.##}";

            yield return null;
        }

        multText.text =
            $"MULT\n×{to:0.##}";

        yield return PopRoutine(
            multText.rectTransform,
            multBaseScale
        );
    }

    public IEnumerator AnimateTotal(
        int from,
        int to
    )
    {
        if (totalScoreText == null)
        {
            yield break;
        }

        yield return AnimateInteger(
            totalScoreText,
            "SCORE",
            from,
            to
        );

        yield return PopRoutine(
            totalScoreText.rectTransform,
            totalBaseScale
        );
    }

    private IEnumerator AnimateInteger(
        TMP_Text target,
        string label,
        int from,
        int to
    )
    {
        float duration =
            Mathf.Max(
                0.01f,
                countDuration
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

            int value =
                Mathf.RoundToInt(
                    Mathf.Lerp(
                        from,
                        to,
                        t
                    )
                );

            target.text =
                $"{label}\n{value}";

            yield return null;
        }

        target.text =
            $"{label}\n{to}";
    }

    private IEnumerator PopRoutine(
        RectTransform target,
        Vector3 baseScale
    )
    {
        if (target == null)
        {
            yield break;
        }

        float half =
            Mathf.Max(
                0.01f,
                popDuration * 0.5f
            );

        Vector3 large =
            baseScale * popScale;

        float elapsed = 0f;

        while (elapsed < half)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / half
                );

            float curved =
                popCurve != null
                    ? popCurve.Evaluate(t)
                    : t;

            target.localScale =
                Vector3.LerpUnclamped(
                    baseScale,
                    large,
                    curved
                );

            yield return null;
        }

        elapsed = 0f;

        while (elapsed < half)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / half
                );

            float curved =
                popCurve != null
                    ? popCurve.Evaluate(t)
                    : t;

            target.localScale =
                Vector3.LerpUnclamped(
                    large,
                    baseScale,
                    curved
                );

            yield return null;
        }

        target.localScale =
            baseScale;
    }

    private void UpdateGoal()
    {
        if (goalText != null)
        {
            goalText.text =
                $"GOAL: {currentGoalScore}";
        }
    }

    private void ResetScales()
    {
        if (pointsText != null)
        {
            pointsText.rectTransform.localScale =
                pointsBaseScale;
        }

        if (multText != null)
        {
            multText.rectTransform.localScale =
                multBaseScale;
        }

        if (totalScoreText != null)
        {
            totalScoreText.rectTransform.localScale =
                totalBaseScale;
        }

        if (goalText != null)
        {
            goalText.rectTransform.localScale =
                goalBaseScale;
        }
    }
}