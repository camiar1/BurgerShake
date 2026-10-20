using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinScreenController : MonoBehaviour
{
    [Header("Game")]
    [SerializeField]
    private RunManager runManager;

    [SerializeField]
    private RunProgress runProgress;

    [SerializeField]
    private ScoreManager scoreManager;

    [Header("Timing")]
    [SerializeField]
    private float showDelay = 0.35f;

    [SerializeField]
    private float showDuration = 0.28f;

    [Header("Scenes")]
    [SerializeField]
    private string mainMenuSceneName = "MainMenu";

    private GameObject screenRoot;
    private RectTransform panelRect;
    private CanvasGroup canvasGroup;

    private TMP_Text customersText;
    private TMP_Text finalScoreText;
    private TMP_Text coinsText;
    private TMP_Text pantryText;
    private TMP_Text helpersText;

    private Button playAgainButton;
    private Button mainMenuButton;

    private Coroutine showRoutine;

    private readonly Color backdropColor =
        new Color(0.08f, 0.045f, 0.025f, 0.9f);

    private readonly Color panelColor =
        new Color(0.98f, 0.89f, 0.71f, 1f);

    private readonly Color inkColor =
        new Color(0.16f, 0.08f, 0.035f, 1f);

    private readonly Color accentColor =
        new Color(0.92f, 0.34f, 0.08f, 1f);

    private readonly Color secondaryButtonColor =
        new Color(0.88f, 0.75f, 0.57f, 1f);

    private void Awake()
    {
        ResolveReferences();
        BuildFallbackUI();
        HideInstant();
    }

    private void OnEnable()
    {
        if (runManager != null)
        {
            runManager.StateChanged += HandleRunStateChanged;
        }

        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(HandlePlayAgainPressed);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(HandleMainMenuPressed);
        }
    }

    private void OnDisable()
    {
        if (runManager != null)
        {
            runManager.StateChanged -= HandleRunStateChanged;
        }

        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveListener(HandlePlayAgainPressed);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(HandleMainMenuPressed);
        }

        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
            showRoutine = null;
        }
    }

    private void ResolveReferences()
    {
        if (runManager == null)
        {
            runManager = FindFirstObjectByType<RunManager>();
        }

        if (runProgress == null)
        {
            runProgress = FindFirstObjectByType<RunProgress>();
        }

        if (scoreManager == null)
        {
            scoreManager = FindFirstObjectByType<ScoreManager>();
        }
    }

    private void HandleRunStateChanged(RunState state)
    {
        if (state != RunState.Won)
        {
            return;
        }

        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
        }

        showRoutine = StartCoroutine(ShowWinScreenRoutine());
    }

    private IEnumerator ShowWinScreenRoutine()
    {
        if (showDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(showDelay);
        }

        UpdateSummary();

        if (screenRoot == null)
        {
            showRoutine = null;
            yield break;
        }

        screenRoot.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;
        }

        if (panelRect != null)
        {
            panelRect.localScale = new Vector3(0.86f, 0.86f, 1f);
        }

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, showDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            float eased = EaseOutBack(t);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = t;
            }

            if (panelRect != null)
            {
                float scale = Mathf.LerpUnclamped(0.86f, 1f, eased);
                panelRect.localScale = new Vector3(scale, scale, 1f);
            }

            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (panelRect != null)
        {
            panelRect.localScale = Vector3.one;
        }

        if (playAgainButton != null)
        {
            playAgainButton.Select();
        }

        showRoutine = null;
    }

    private void UpdateSummary()
    {
        int totalCustomers = 0;

        if (
            runManager != null &&
            runManager.Definition != null &&
            runManager.Definition.customers != null
        )
        {
            totalCustomers = runManager.Definition.customers.Count;
        }

        int customersServed = totalCustomers;

        if (runProgress != null && totalCustomers <= 0)
        {
            customersServed = Mathf.Max(0, runProgress.Day);
        }

        if (customersText != null)
        {
            customersText.text =
                totalCustomers > 0
                    ? $"{customersServed}/{totalCustomers} CUSTOMERS SERVED"
                    : $"{customersServed} CUSTOMERS SERVED";
        }

        if (finalScoreText != null)
        {
            int score = scoreManager != null
                ? scoreManager.TotalScore
                : 0;

            finalScoreText.text = $"FINAL SHAKE  {score}";
        }

        if (coinsText != null)
        {
            int coins = runProgress != null
                ? runProgress.Coins
                : 0;

            coinsText.text = $"COINS LEFT  ${coins}";
        }

        if (pantryText != null)
        {
            pantryText.text = BuildPantrySummary();
        }

        if (helpersText != null)
        {
            helpersText.text = BuildHelperSummary();
        }
    }

    private string BuildPantrySummary()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("PANTRY");

        if (
            runProgress == null ||
            runProgress.Pantry == null ||
            runProgress.Pantry.Count == 0
        )
        {
            builder.Append("None");
            return builder.ToString();
        }

        int shown = 0;

        foreach (RunIngredientEntry entry in runProgress.Pantry)
        {
            if (entry == null || entry.Ingredient == null)
            {
                continue;
            }

            if (shown >= 10)
            {
                builder.AppendLine("...");
                break;
            }

            builder.Append(entry.Ingredient.ingredientName);
            builder.Append("  x");
            builder.AppendLine(entry.Copies.ToString());
            shown++;
        }

        return builder.ToString().TrimEnd();
    }

    private string BuildHelperSummary()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("HELPERS");

        if (
            runProgress == null ||
            runProgress.Upgrades == null ||
            runProgress.Upgrades.Count == 0
        )
        {
            builder.Append("None");
            return builder.ToString();
        }

        int shown = 0;

        foreach (RunUpgradeDefinition upgrade in runProgress.Upgrades)
        {
            if (upgrade == null)
            {
                continue;
            }

            if (shown >= 10)
            {
                builder.AppendLine("...");
                break;
            }

            string helperName = string.IsNullOrWhiteSpace(upgrade.upgradeName)
                ? upgrade.name
                : upgrade.upgradeName;

            builder.AppendLine(helperName);
            shown++;
        }

        return builder.ToString().TrimEnd();
    }

    private void HideInstant()
    {
        if (screenRoot != null)
        {
            screenRoot.SetActive(false);
        }
    }

    private void HandlePlayAgainPressed()
    {
        Time.timeScale = 1f;

        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    private void HandleMainMenuPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void BuildFallbackUI()
    {
        if (screenRoot != null)
        {
            return;
        }

        screenRoot = new GameObject(
            "WinScreenCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(CanvasGroup)
        );

        screenRoot.transform.SetParent(transform, false);

        Canvas canvas = screenRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 2000;

        CanvasScaler scaler = screenRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGroup = screenRoot.GetComponent<CanvasGroup>();

        RectTransform canvasRect = screenRoot.GetComponent<RectTransform>();
        StretchFullScreen(canvasRect);

        RectTransform backdrop = CreateRect(
            "Backdrop",
            canvasRect,
            Vector2.zero,
            Vector2.zero
        );
        StretchFullScreen(backdrop);

        Image backdropImage = backdrop.gameObject.AddComponent<Image>();
        backdropImage.color = backdropColor;
        backdropImage.raycastTarget = true;

        panelRect = CreateRect(
            "WinPanel",
            canvasRect,
            Vector2.zero,
            new Vector2(930f, 790f)
        );

        Image panelImage = panelRect.gameObject.AddComponent<Image>();
        panelImage.color = panelColor;
        panelImage.raycastTarget = true;

        CreateText(
            "Title",
            panelRect,
            "SHIFT COMPLETE!",
            new Vector2(0f, 315f),
            new Vector2(820f, 90f),
            58f,
            accentColor,
            FontStyles.Bold
        );

        CreateText(
            "Subtitle",
            panelRect,
            "THE TRUCK SURVIVED ANOTHER NIGHT",
            new Vector2(0f, 258f),
            new Vector2(760f, 48f),
            23f,
            inkColor,
            FontStyles.Normal
        );

        customersText = CreateText(
            "CustomersText",
            panelRect,
            "3/3 CUSTOMERS SERVED",
            new Vector2(0f, 192f),
            new Vector2(780f, 45f),
            30f,
            inkColor,
            FontStyles.Bold
        );

        finalScoreText = CreateText(
            "FinalScoreText",
            panelRect,
            "FINAL SHAKE  0",
            new Vector2(-205f, 137f),
            new Vector2(360f, 45f),
            27f,
            inkColor,
            FontStyles.Bold
        );

        coinsText = CreateText(
            "CoinsText",
            panelRect,
            "COINS LEFT  $0",
            new Vector2(205f, 137f),
            new Vector2(360f, 45f),
            27f,
            inkColor,
            FontStyles.Bold
        );

        RectTransform divider = CreateRect(
            "Divider",
            panelRect,
            new Vector2(0f, 94f),
            new Vector2(760f, 4f)
        );

        Image dividerImage = divider.gameObject.AddComponent<Image>();
        dividerImage.color = inkColor;
        dividerImage.raycastTarget = false;

        pantryText = CreateText(
            "PantryText",
            panelRect,
            "PANTRY",
            new Vector2(-215f, -55f),
            new Vector2(365f, 280f),
            24f,
            inkColor,
            FontStyles.Normal,
            TextAlignmentOptions.TopLeft
        );

        helpersText = CreateText(
            "HelpersText",
            panelRect,
            "HELPERS",
            new Vector2(215f, -55f),
            new Vector2(365f, 280f),
            24f,
            inkColor,
            FontStyles.Normal,
            TextAlignmentOptions.TopLeft
        );

        playAgainButton = CreateButton(
            "PlayAgainButton",
            panelRect,
            "PLAY AGAIN",
            new Vector2(-175f, -315f),
            new Vector2(300f, 78f),
            accentColor,
            panelColor
        );

        mainMenuButton = CreateButton(
            "MainMenuButton",
            panelRect,
            "MAIN MENU",
            new Vector2(175f, -315f),
            new Vector2(300f, 78f),
            secondaryButtonColor,
            inkColor
        );
    }

    private RectTransform CreateRect(
        string objectName,
        Transform parent,
        Vector2 anchoredPosition,
        Vector2 size
    )
    {
        GameObject gameObject = new GameObject(
            objectName,
            typeof(RectTransform)
        );

        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        return rect;
    }

    private TMP_Text CreateText(
        string objectName,
        Transform parent,
        string value,
        Vector2 anchoredPosition,
        Vector2 size,
        float fontSize,
        Color color,
        FontStyles fontStyle,
        TextAlignmentOptions alignment = TextAlignmentOptions.Center
    )
    {
        RectTransform rect = CreateRect(
            objectName,
            parent,
            anchoredPosition,
            size
        );

        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        text.overflowMode = TextOverflowModes.Overflow;

        if (TMP_Settings.defaultFontAsset != null)
        {
            text.font = TMP_Settings.defaultFontAsset;
        }

        return text;
    }

    private Button CreateButton(
        string objectName,
        Transform parent,
        string label,
        Vector2 anchoredPosition,
        Vector2 size,
        Color normalColor,
        Color textColor
    )
    {
        RectTransform rect = CreateRect(
            objectName,
            parent,
            anchoredPosition,
            size
        );

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = normalColor;

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = Color.Lerp(normalColor, Color.white, 0.16f);
        colors.pressedColor = Color.Lerp(normalColor, Color.black, 0.16f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(
            normalColor.r,
            normalColor.g,
            normalColor.b,
            0.45f
        );
        button.colors = colors;

        button.gameObject.AddComponent<UIButtonAnimator>();

        CreateText(
            "Label",
            rect,
            label,
            Vector2.zero,
            size,
            27f,
            textColor,
            FontStyles.Bold
        );

        return button;
    }

    private void StretchFullScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
    }

    private float EaseOutBack(float t)
    {
        const float overshoot = 1.70158f;
        const float amount = overshoot + 1f;

        float value = t - 1f;

        return
            1f +
            amount * value * value * value +
            overshoot * value * value;
    }
}

internal static class WinScreenBootstrap
{
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad
    )]
    private static void CreateIfNeeded()
    {
        RunManager runManager =
            Object.FindFirstObjectByType<RunManager>();

        if (runManager == null)
        {
            return;
        }

        WinScreenController existing =
            Object.FindFirstObjectByType<WinScreenController>();

        if (existing != null)
        {
            return;
        }

        GameObject controllerObject =
            new GameObject("[Runtime] Win Screen Controller");

        controllerObject.AddComponent<WinScreenController>();
    }
}
