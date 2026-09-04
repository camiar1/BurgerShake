using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Audio")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Display")]
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Controls")]
    [SerializeField] private Button backButton;

    public bool IsOpen =>
        settingsPanel != null &&
        settingsPanel.activeSelf;

    public event Action Closed;

    private void OnEnable()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged
                .AddListener(
                    HandleMasterVolumeChanged
                );
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged
                .AddListener(
                    HandleMusicVolumeChanged
                );
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged
                .AddListener(
                    HandleSFXVolumeChanged
                );
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged
                .AddListener(
                    HandleFullscreenChanged
                );
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(
                Hide
            );
        }
    }

    private void OnDisable()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged
                .RemoveListener(
                    HandleMasterVolumeChanged
                );
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged
                .RemoveListener(
                    HandleMusicVolumeChanged
                );
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged
                .RemoveListener(
                    HandleSFXVolumeChanged
                );
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged
                .RemoveListener(
                    HandleFullscreenChanged
                );
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(
                Hide
            );
        }
    }

    private void Start()
    {
        SetPanelVisible(false);
    }

    public void Show()
    {
        SetPanelVisible(true);

        RefreshUI();
    }

    public void Hide()
    {
        SetPanelVisible(false);

        Closed?.Invoke();
    }

    private void RefreshUI()
    {
        SettingsManager manager =
            GetSettingsManager();

        if (manager == null)
        {
            return;
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(
                manager.MasterVolume
            );
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.SetValueWithoutNotify(
                manager.MusicVolume
            );
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(
                manager.SFXVolume
            );
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(
                manager.Fullscreen
            );
        }
    }

    private void HandleMasterVolumeChanged(
        float value
    )
    {
        GetSettingsManager()
            ?.SetMasterVolume(value);
    }

    private void HandleMusicVolumeChanged(
        float value
    )
    {
        GetSettingsManager()
            ?.SetMusicVolume(value);
    }

    private void HandleSFXVolumeChanged(
        float value
    )
    {
        GetSettingsManager()
            ?.SetSFXVolume(value);
    }

    private void HandleFullscreenChanged(
        bool fullscreen
    )
    {
        GetSettingsManager()
            ?.SetFullscreen(fullscreen);
    }

    private SettingsManager GetSettingsManager()
    {
        if (SettingsManager.Instance != null)
        {
            return SettingsManager.Instance;
        }

        return FindFirstObjectByType<
            SettingsManager
        >();
    }

    private void SetPanelVisible(
        bool visible
    )
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(
                visible
            );
        }
    }
}