using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoseScreenController : MonoBehaviour
{
    [Header("Game")]
    [SerializeField]
    private RunManager runManager;

    [SerializeField]
    private RunProgress runProgress;

    [SerializeField]
    private ScoreManager scoreManager;

    [Header("Screen")]
    [SerializeField]
    private UIPanelAnimator loseScreenAnimator;

    [Header("Text")]
    [SerializeField]
    private TMP_Text finalScoreText;

    [SerializeField]
    private TMP_Text goalText;

    [SerializeField]
    private TMP_Text dayText;

    [Header("Buttons")]
    [SerializeField]
    private Button retryButton;

    [SerializeField]
    private Button mainMenuButton;

    [Header("Timing")]
    [SerializeField]
    private float showDelay = 0.35f;

    [Header("Scenes")]
    [SerializeField]
    private string mainMenuSceneName =
        "MainMenu";

    private Coroutine showRoutine;

    private void Awake()
    {
        if (runManager == null)
        {
            runManager =
                FindFirstObjectByType<
                    RunManager
                >();
        }

        if (runProgress == null)
        {
            runProgress =
                FindFirstObjectByType<
                    RunProgress
                >();
        }

        if (scoreManager == null)
        {
            scoreManager =
                FindFirstObjectByType<
                    ScoreManager
                >();
        }

        // Always start with the lose screen hidden.
        if (loseScreenAnimator != null)
        {
            loseScreenAnimator
                .HideInstant();
        }
    }

    private void OnEnable()
    {
        if (runManager != null)
        {
            runManager.StateChanged +=
                HandleRunStateChanged;
        }

        if (retryButton != null)
        {
            retryButton.onClick
                .AddListener(
                    HandleRetryPressed
                );
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick
                .AddListener(
                    HandleMainMenuPressed
                );
        }
    }

    private void OnDisable()
    {
        if (runManager != null)
        {
            runManager.StateChanged -=
                HandleRunStateChanged;
        }

        if (retryButton != null)
        {
            retryButton.onClick
                .RemoveListener(
                    HandleRetryPressed
                );
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick
                .RemoveListener(
                    HandleMainMenuPressed
                );
        }

        if (showRoutine != null)
        {
            StopCoroutine(
                showRoutine
            );

            showRoutine =
                null;
        }
    }

    private void HandleRunStateChanged(
        RunState state
    )
    {
        if (state != RunState.Lost)
        {
            return;
        }

        if (showRoutine != null)
        {
            StopCoroutine(
                showRoutine
            );
        }

        showRoutine =
            StartCoroutine(
                ShowLoseScreenRoutine()
            );
    }

    private IEnumerator
        ShowLoseScreenRoutine()
    {
        if (showDelay > 0f)
        {
            yield return
                new WaitForSecondsRealtime(
                    showDelay
                );
        }

        UpdateText();

        if (loseScreenAnimator != null)
        {
            loseScreenAnimator.Show();
        }

        showRoutine =
            null;
    }

    private void UpdateText()
    {
        int finalScore =
            scoreManager != null
                ? scoreManager.TotalScore
                : 0;

        int goal =
            runManager != null
                ? runManager.CurrentGoalScore
                : 0;

        int day =
            runProgress != null
                ? runProgress.Day
                : 1;

        if (finalScoreText != null)
        {
            finalScoreText.text =
                $"FINAL SCORE\n{finalScore}";
        }

        if (goalText != null)
        {
            goalText.text =
                $"GOAL\n{goal}";
        }

        if (dayText != null)
        {
            dayText.text =
                $"DAY {day}";
        }
    }

    private void HandleRetryPressed()
    {
        Time.timeScale =
            1f;

        Scene scene =
            SceneManager
                .GetActiveScene();

        SceneManager.LoadScene(
            scene.name
        );
    }

    private void HandleMainMenuPressed()
    {
        Time.timeScale =
            1f;

        SceneManager.LoadScene(
            mainMenuSceneName
        );
    }
}