using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private RunManager runManager;

    [Header("Score Text")]
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private TMP_Text multText;
    [SerializeField] private TMP_Text totalScoreText;
    [SerializeField] private TMP_Text goalText;

    [Header("Animation")]
    [SerializeField] private float popScale = 1.2f;
    [SerializeField] private float popDuration = 0.18f;

    [SerializeField]
    private AnimationCurve popCurve =
        AnimationCurve.EaseInOut(
            0f,
            0f,
            1f,
            1f
        );

    private int currentGoalScore;

    private int previousPoints;
    private float previousMult;
    private int previousTotalScore;

    private bool hasPreviousScore;

    private Coroutine pointsPopRoutine;
    private Coroutine multPopRoutine;
    private Coroutine totalPopRoutine;
    private Coroutine goalPopRoutine;

    private Vector3 pointsBaseScale;
    private Vector3 multBaseScale;
    private Vector3 totalBaseScale;
    private Vector3 goalBaseScale;

    private void Awake()
    {
        if (scoreManager == null)
        {
            scoreManager =
                FindFirstObjectByType<ScoreManager>();
        }

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
        if (scoreManager != null)
        {
            scoreManager.ScoreChanged +=
                HandleScoreChanged;
        }

        if (runManager != null)
        {
            runManager.CustomerIntroStarted +=
                HandleCustomerIntroStarted;
        }
    }

    private void OnDisable()
    {
        if (scoreManager != null)
        {
            scoreManager.ScoreChanged -=
                HandleScoreChanged;
        }

        if (runManager != null)
        {
            runManager.CustomerIntroStarted -=
                HandleCustomerIntroStarted;
        }

        ResetTextScales();
    }

    private void Start()
    {
        if (runManager != null)
        {
            currentGoalScore =
                runManager.CurrentGoalScore;
        }

        RefreshUI(false);
    }

    private void HandleScoreChanged()
    {
        RefreshUI(true);
    }

    private void HandleCustomerIntroStarted(
        CustomerDefinition customer,
        int goalScore
    )
    {
        bool goalChanged =
            currentGoalScore != goalScore;

        currentGoalScore =
            goalScore;

        RefreshUI(false);

        if (goalChanged)
        {
            StartGoalPop();
        }
    }

    private void RefreshUI(
        bool animateChanges
    )
    {
        if (scoreManager == null)
        {
            return;
        }

        int newPoints =
            scoreManager.Points;

        float newMult =
            scoreManager.Mult;

        int newTotalScore =
            scoreManager.TotalScore;

        if (pointsText != null)
        {
            pointsText.text =
                $"POINTS\n{newPoints}";
        }

        if (multText != null)
        {
            multText.text =
                $"MULT\n×{newMult:0.##}";
        }

        if (totalScoreText != null)
        {
            totalScoreText.text =
                $"SCORE\n{newTotalScore}";
        }

        if (goalText != null)
        {
            goalText.text =
                $"GOAL: {currentGoalScore}";
        }

        if (
            animateChanges &&
            hasPreviousScore
        )
        {
            if (
                newPoints !=
                previousPoints
            )
            {
                StartPointsPop();
            }

            if (
                !Mathf.Approximately(
                    newMult,
                    previousMult
                )
            )
            {
                StartMultPop();
            }

            if (
                newTotalScore !=
                previousTotalScore
            )
            {
                StartTotalPop();
            }
        }

        previousPoints =
            newPoints;

        previousMult =
            newMult;

        previousTotalScore =
            newTotalScore;

        hasPreviousScore = true;
    }

    private void StartPointsPop()
    {
        if (pointsText == null)
        {
            return;
        }

        if (pointsPopRoutine != null)
        {
            StopCoroutine(
                pointsPopRoutine
            );
        }

        pointsText.rectTransform.localScale =
            pointsBaseScale;

        pointsPopRoutine =
            StartCoroutine(
                PopRoutine(
                    pointsText.rectTransform,
                    pointsBaseScale
                )
            );
    }

    private void StartMultPop()
    {
        if (multText == null)
        {
            return;
        }

        if (multPopRoutine != null)
        {
            StopCoroutine(
                multPopRoutine
            );
        }

        multText.rectTransform.localScale =
            multBaseScale;

        multPopRoutine =
            StartCoroutine(
                PopRoutine(
                    multText.rectTransform,
                    multBaseScale
                )
            );
    }

    private void StartTotalPop()
    {
        if (totalScoreText == null)
        {
            return;
        }

        if (totalPopRoutine != null)
        {
            StopCoroutine(
                totalPopRoutine
            );
        }

        totalScoreText.rectTransform.localScale =
            totalBaseScale;

        totalPopRoutine =
            StartCoroutine(
                PopRoutine(
                    totalScoreText.rectTransform,
                    totalBaseScale
                )
            );
    }

    private void StartGoalPop()
    {
        if (goalText == null)
        {
            return;
        }

        if (goalPopRoutine != null)
        {
            StopCoroutine(
                goalPopRoutine
            );
        }

        goalText.rectTransform.localScale =
            goalBaseScale;

        goalPopRoutine =
            StartCoroutine(
                PopRoutine(
                    goalText.rectTransform,
                    goalBaseScale
                )
            );
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

        float halfDuration =
            Mathf.Max(
                0.01f,
                popDuration * 0.5f
            );

        Vector3 enlargedScale =
            baseScale * popScale;

        float elapsed = 0f;

        while (elapsed < halfDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float normalized =
                Mathf.Clamp01(
                    elapsed / halfDuration
                );

            float curved =
                popCurve != null
                    ? popCurve.Evaluate(
                        normalized
                    )
                    : normalized;

            target.localScale =
                Vector3.LerpUnclamped(
                    baseScale,
                    enlargedScale,
                    curved
                );

            yield return null;
        }

        elapsed = 0f;

        while (elapsed < halfDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float normalized =
                Mathf.Clamp01(
                    elapsed / halfDuration
                );

            float curved =
                popCurve != null
                    ? popCurve.Evaluate(
                        normalized
                    )
                    : normalized;

            target.localScale =
                Vector3.LerpUnclamped(
                    enlargedScale,
                    baseScale,
                    curved
                );

            yield return null;
        }

        target.localScale =
            baseScale;
    }

    private void ResetTextScales()
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