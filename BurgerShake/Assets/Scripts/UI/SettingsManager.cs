using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance
    {
        get;
        private set;
    }

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;

    private const string MasterVolumeKey =
        "MasterVolume";

    private const string MusicVolumeKey =
        "MusicVolume";

    private const string SFXVolumeKey =
        "SFXVolume";

    private const string FullscreenKey =
        "Fullscreen";

    public float MasterVolume
    {
        get;
        private set;
    } = 1f;

    public float MusicVolume
    {
        get;
        private set;
    } = 1f;

    public float SFXVolume
    {
        get;
        private set;
    } = 1f;

    public bool Fullscreen
    {
        get;
        private set;
    }

    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(
            gameObject
        );

        LoadSettings();
    }

    public void SetMasterVolume(
        float value
    )
    {
        MasterVolume =
            Mathf.Clamp01(value);

        ApplyMixerVolume(
            "MasterVolume",
            MasterVolume
        );

        PlayerPrefs.SetFloat(
            MasterVolumeKey,
            MasterVolume
        );

        PlayerPrefs.Save();
    }

    public void SetMusicVolume(
        float value
    )
    {
        MusicVolume =
            Mathf.Clamp01(value);

        ApplyMixerVolume(
            "MusicVolume",
            MusicVolume
        );

        PlayerPrefs.SetFloat(
            MusicVolumeKey,
            MusicVolume
        );

        PlayerPrefs.Save();
    }

    public void SetSFXVolume(
        float value
    )
    {
        SFXVolume =
            Mathf.Clamp01(value);

        ApplyMixerVolume(
            "SFXVolume",
            SFXVolume
        );

        PlayerPrefs.SetFloat(
            SFXVolumeKey,
            SFXVolume
        );

        PlayerPrefs.Save();
    }

    public void SetFullscreen(
        bool fullscreen
    )
    {
        Fullscreen =
            fullscreen;

        Screen.fullScreen =
            fullscreen;

        PlayerPrefs.SetInt(
            FullscreenKey,
            fullscreen ? 1 : 0
        );

        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        MasterVolume =
            PlayerPrefs.GetFloat(
                MasterVolumeKey,
                1f
            );

        MusicVolume =
            PlayerPrefs.GetFloat(
                MusicVolumeKey,
                1f
            );

        SFXVolume =
            PlayerPrefs.GetFloat(
                SFXVolumeKey,
                1f
            );

        Fullscreen =
            PlayerPrefs.GetInt(
                FullscreenKey,
                Screen.fullScreen ? 1 : 0
            ) == 1;

        ApplyMixerVolume(
            "MasterVolume",
            MasterVolume
        );

        ApplyMixerVolume(
            "MusicVolume",
            MusicVolume
        );

        ApplyMixerVolume(
            "SFXVolume",
            SFXVolume
        );

        Screen.fullScreen =
            Fullscreen;
    }

    private void ApplyMixerVolume(
        string parameter,
        float linearValue
    )
    {
        if (audioMixer == null)
        {
            return;
        }

        float decibels;

        if (linearValue <= 0.0001f)
        {
            decibels = -80f;
        }
        else
        {
            decibels =
                Mathf.Log10(
                    linearValue
                ) * 20f;
        }

        audioMixer.SetFloat(
            parameter,
            decibels
        );
    }
}