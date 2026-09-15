using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private SettingsUI settingsUI;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Scenes")]
    [SerializeField]
    private string gameSceneName =
        "SampleScene";

    private void OnEnable()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(
                HandlePlayPressed
            );
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(
                HandleSettingsPressed
            );
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(
                HandleQuitPressed
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
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(
                HandlePlayPressed
            );
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(
                HandleSettingsPressed
            );
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(
                HandleQuitPressed
            );
        }

        if (settingsUI != null)
        {
            settingsUI.Closed -=
                HandleSettingsClosed;
        }
    }

    private void HandlePlayPressed()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            gameSceneName
        );
    }

    private void HandleSettingsPressed()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(
                false
            );
        }

        settingsUI?.Show();
    }

    private void HandleSettingsClosed()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(
                true
            );
        }
    }

    private void HandleQuitPressed()
    {
        Application.Quit();

#if UNITY_EDITOR
        Debug.Log(
            "Quit pressed. Application.Quit only closes a built game."
        );
#endif
    }
}