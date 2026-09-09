using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("Game")]
    [SerializeField] private RunManager runManager;
    [SerializeField] private ViewController viewController;
    [SerializeField] private IngredientDropper ingredientDropper;

    [Header("Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private SettingsUI settingsUI;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Scenes")]
    [SerializeField]
    private string mainMenuSceneName =
        "MainMenu";

    public bool IsPaused
    {
        get;
        private set;
    }

    private bool isResuming;

    private void Awake()
    {
        if (runManager == null)
        {
            runManager =
                FindFirstObjectByType<RunManager>();
        }

        if (viewController == null)
        {
            viewController =
                FindFirstObjectByType<ViewController>();
        }

        if (ingredientDropper == null)
        {
            ingredientDropper =
                FindFirstObjectByType<IngredientDropper>();
        }
    }

    private void OnEnable()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(
                ResumeGame
            );
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(
                HandleSettingsPressed
            );
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(
                HandleMainMenuPressed
            );
        }

        if (settingsUI != null)
        {
            settingsUI.Closed +=
                HandleSettingsClosed;
        }
    }

    private void OnDisable()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(
                ResumeGame
            );
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(
                HandleSettingsPressed
            );
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(
                HandleMainMenuPressed
            );
        }

        if (settingsUI != null)
        {
            settingsUI.Closed -=
                HandleSettingsClosed;
        }
    }

    private void Start()
    {
        SetPauseMenuVisible(false);

        IsPaused = false;

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (
            Keyboard.current == null ||
            !Keyboard.current.escapeKey
                .wasPressedThisFrame
        )
        {
            return;
        }

        if (IsPaused)
        {
            if (
                settingsUI != null &&
                settingsUI.IsOpen
            )
            {
                settingsUI.Hide();
            }
            else
            {
                ResumeGame();
            }

            return;
        }

        TryPauseGame();
    }

    public void TryPauseGame()
    {
        if (!CanPause())
        {
            return;
        }

        IsPaused = true;

        Time.timeScale = 0f;

        if (ingredientDropper != null)
        {
            ingredientDropper.enabled =
                false;
        }

        SetPauseMenuVisible(true);
    }

    public void ResumeGame()
    {
        if (!IsPaused)
        {
            return;
        }

        isResuming = true;

        if (
            settingsUI != null &&
            settingsUI.IsOpen
        )
        {
            settingsUI.Hide();
        }

        SetPauseMenuVisible(false);

        Time.timeScale = 1f;

        if (ingredientDropper != null)
        {
            ingredientDropper.enabled =
                true;
        }

        IsPaused = false;

        isResuming = false;
    }

    private bool CanPause()
    {
        if (
            viewController != null &&
            viewController.IsSliding
        )
        {
            return false;
        }

        if (
            runManager != null &&
            runManager.State !=
                RunState.Assembly
        )
        {
            return false;
        }

        return true;
    }

    private void HandleSettingsPressed()
    {
        SetPauseMenuVisible(false);

        settingsUI?.Show();
    }

    private void HandleSettingsClosed()
    {
        if (
            IsPaused &&
            !isResuming
        )
        {
            SetPauseMenuVisible(true);
        }
    }

    private void HandleMainMenuPressed()
    {
        Time.timeScale = 1f;

        if (ingredientDropper != null)
        {
            ingredientDropper.enabled =
                true;
        }

        IsPaused = false;

        SceneManager.LoadScene(
            mainMenuSceneName
        );
    }

    private void SetPauseMenuVisible(
        bool visible
    )
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(
                visible
            );
        }
    }

    private void OnDestroy()
    {
        if (IsPaused)
        {
            Time.timeScale = 1f;
        }
    }
}