using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class RunStatusHUDController : MonoBehaviour
{
    [Header("Run")]
    [SerializeField] private RunManager runManager;
    [SerializeField] private RunProgress runProgress;
    [SerializeField] private CustomerChallengeController challengeController;
    [SerializeField] private CatTossDraftController tossController;

    private CanvasGroup canvasGroup;
    private TMP_Text customerText;
    private TMP_Text resourcesText;

    private const float Width = 620f;
    private const float Height = 92f;

    private void Awake()
    {
        ResolveReferences();
        BuildUI();
        Refresh();
    }

    private void OnEnable()
    {
        ResolveReferences();
        BuildUI();
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void ResolveReferences()
    {
        if (runManager == null)
            runManager = FindFirstObjectByType<RunManager>();

        if (runProgress == null)
            runProgress = FindFirstObjectByType<RunProgress>();

        if (challengeController == null)
            challengeController = FindFirstObjectByType<CustomerChallengeController>();

        if (tossController == null)
            tossController = FindFirstObjectByType<CatTossDraftController>();
    }

    private void Refresh()
    {
        if (canvasGroup == null || customerText == null || resourcesText == null)
            return;

        if (!Application.isPlaying)
        {
            canvasGroup.alpha = 1f;
            customerText.text = "CUSTOMER 4/10  •  CHLOE    GOAL 48";
            resourcesText.text = "COINS $14    DISPENSES 4/6";
            return;
        }

        bool visible =
            runManager != null &&
            runManager.State == RunState.Assembly;

        canvasGroup.alpha = visible ? 1f : 0f;

        if (!visible)
            return;

        int day = runProgress != null ? Mathf.Max(1, runProgress.Day) : 1;
        int totalCustomers =
            runManager.Definition != null &&
            runManager.Definition.customers != null
                ? runManager.Definition.customers.Count
                : 0;

        string customerName =
            challengeController != null &&
            challengeController.CurrentCustomer != null
                ? challengeController.CurrentCustomer.customerName.ToUpperInvariant()
                : "CUSTOMER";

        int goal =
            challengeController != null
                ? challengeController.GoalScore
                : 0;

        int coins =
            runProgress != null
                ? runProgress.Coins
                : 0;

        int remaining =
            tossController != null
                ? tossController.DispensesRemaining
                : 0;

        int totalDispenses =
            tossController != null
                ? tossController.DispensesPerRound
                : 0;

        string progressLabel = totalCustomers > 0
            ? $"CUSTOMER {day}/{totalCustomers}"
            : $"CUSTOMER {day}";

        customerText.text =
            $"{progressLabel}  •  {customerName}    GOAL {goal}";

        resourcesText.text =
            $"COINS ${coins}    DISPENSES {remaining}/{totalDispenses}";
    }

    private void BuildUI()
    {
        Transform existingCanvas = transform.Find("RunStatusCanvas");

        if (existingCanvas != null)
        {
            ResolveExistingUI(existingCanvas);

            if (canvasGroup != null && customerText != null && resourcesText != null)
                return;
        }

        GameObject canvasObject = new GameObject(
            "RunStatusCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(CanvasGroup)
        );

        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1200;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGroup = canvasObject.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        RectTransform panel = CreateRect(
            "RunStatusPanel",
            canvasObject.transform,
            new Vector2(26f, -26f),
            new Vector2(Width, Height)
        );

        panel.anchorMin = new Vector2(0f, 1f);
        panel.anchorMax = new Vector2(0f, 1f);
        panel.pivot = new Vector2(0f, 1f);

        Image background = panel.gameObject.AddComponent<Image>();
        background.color = new Color(0.07f, 0.045f, 0.025f, 0.9f);
        background.raycastTarget = false;

        customerText = CreateText(
            "CustomerStatus",
            panel,
            new Vector2(18f, -14f),
            new Vector2(Width - 36f, 34f),
            23f,
            FontStyles.Bold
        );

        resourcesText = CreateText(
            "ResourcesStatus",
            panel,
            new Vector2(18f, -50f),
            new Vector2(Width - 36f, 28f),
            19f,
            FontStyles.Normal
        );
    }

    private void ResolveExistingUI(Transform existingCanvas)
    {
        canvasGroup = existingCanvas.GetComponent<CanvasGroup>();

        Transform panel = existingCanvas.Find("RunStatusPanel");
        if (panel == null)
            return;

        Transform customer = panel.Find("CustomerStatus");
        Transform resources = panel.Find("ResourcesStatus");

        customerText = customer != null
            ? customer.GetComponent<TMP_Text>()
            : null;

        resourcesText = resources != null
            ? resources.GetComponent<TMP_Text>()
            : null;
    }

    private RectTransform CreateRect(
        string objectName,
        Transform parent,
        Vector2 position,
        Vector2 size
    )
    {
        GameObject obj = new GameObject(objectName, typeof(RectTransform));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private TMP_Text CreateText(
        string objectName,
        Transform parent,
        Vector2 position,
        Vector2 size,
        float fontSize,
        FontStyles style
    )
    {
        RectTransform rect = CreateRect(
            objectName,
            parent,
            position,
            size
        );

        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = new Color(1f, 0.94f, 0.82f, 1f);
        text.alignment = TextAlignmentOptions.TopLeft;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;

        return text;
    }
}
