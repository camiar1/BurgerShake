using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ShopBuildSummaryUI : MonoBehaviour
{
    [SerializeField] private RunManager runManager;
    [SerializeField] private RunProgress runProgress;
    [SerializeField] private ShopManager shopManager;

    private CanvasGroup canvasGroup;
    private TMP_Text titleText;
    private TMP_Text bodyText;
    private Button rerollButton;
    private TMP_Text rerollButtonText;

    private const float Width = 500f;
    private const float Height = 360f;

    private void Awake()
    {
        ResolveReferences();
        BuildUI();
        BindRerollButton();
        Refresh();
    }

    private void OnEnable()
    {
        ResolveReferences();
        BuildUI();
        BindRerollButton();
        Refresh();
    }

    private void OnDisable()
    {
        if (rerollButton != null)
            rerollButton.onClick.RemoveListener(HandleRerollPressed);
    }

    public void EnsureEditorLayout()
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

        if (shopManager == null)
            shopManager = FindFirstObjectByType<ShopManager>();
    }

    private void Refresh()
    {
        if (canvasGroup == null || titleText == null || bodyText == null)
            return;

        if (!Application.isPlaying)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            titleText.text = "YOUR BUILD";
            bodyText.text =
                "Helpers: Extra Scoop, Head Start\n" +
                "+1 Dispense  •  +0.15 Starting Mult\n" +
                "Pantry: 6 types  •  18 total copies\n\n" +
                "NEXT: CHLOE  •  GOAL 48";

            if (rerollButtonText != null)
                rerollButtonText.text = "REROLL OFFERS  $2";

            return;
        }

        bool visible =
            runManager != null &&
            runManager.State == RunState.Shop;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = visible;
        canvasGroup.interactable = visible;

        if (!visible)
            return;

        titleText.text = "YOUR BUILD";
        bodyText.text = BuildSummary();
        RefreshRerollButton();
    }

    private string BuildSummary()
    {
        StringBuilder builder = new StringBuilder();

        AppendHelperNames(builder);
        AppendEffects(builder);
        AppendPantry(builder);
        AppendNextCustomer(builder);

        return builder.ToString().TrimEnd();
    }

    private void AppendHelperNames(StringBuilder builder)
    {
        builder.Append("Helpers: ");

        if (runProgress == null ||
            runProgress.Upgrades == null ||
            runProgress.Upgrades.Count == 0)
        {
            builder.AppendLine("None yet");
            return;
        }

        bool first = true;

        foreach (RunUpgradeDefinition upgrade in runProgress.Upgrades)
        {
            if (upgrade == null)
                continue;

            if (!first)
                builder.Append(", ");

            builder.Append(
                string.IsNullOrWhiteSpace(upgrade.upgradeName)
                    ? upgrade.name
                    : upgrade.upgradeName
            );

            first = false;
        }

        builder.AppendLine();
    }

    private void AppendEffects(StringBuilder builder)
    {
        if (runProgress == null)
            return;

        int dispenseBonus = 0;
        int startingPoints = 0;
        int bonusCoins = 0;
        int draftBonus = 0;
        float startingMult = 0f;
        float blenderScale = 1f;
        float ingredientScale = 1f;

        foreach (RunUpgradeDefinition upgrade in runProgress.Upgrades)
        {
            if (upgrade == null)
                continue;

            switch (upgrade.effectType)
            {
                case RunUpgradeEffectType.DispenseBonus:
                    dispenseBonus += Mathf.RoundToInt(upgrade.amount);
                    break;
                case RunUpgradeEffectType.StartingPointsBonus:
                    startingPoints += Mathf.RoundToInt(upgrade.amount);
                    break;
                case RunUpgradeEffectType.StartingMultBonus:
                    startingMult += upgrade.amount;
                    break;
                case RunUpgradeEffectType.BonusCoinsPerWin:
                    bonusCoins += Mathf.RoundToInt(upgrade.amount);
                    break;
                case RunUpgradeEffectType.DraftChoiceBonus:
                    draftBonus += Mathf.RoundToInt(upgrade.amount);
                    break;
                case RunUpgradeEffectType.BlenderScaleMultiplier:
                    blenderScale *= upgrade.amount;
                    break;
                case RunUpgradeEffectType.IngredientScaleMultiplier:
                    ingredientScale *= upgrade.amount;
                    break;
            }
        }

        bool any = false;

        if (dispenseBonus != 0)
        {
            AppendEffect(builder, ref any, $"+{dispenseBonus} Dispense");
        }

        if (startingMult > 0f)
        {
            AppendEffect(builder, ref any, $"+{startingMult:0.##} Starting Mult");
        }

        if (startingPoints > 0)
        {
            AppendEffect(builder, ref any, $"+{startingPoints} Starting Points");
        }

        if (bonusCoins > 0)
        {
            AppendEffect(builder, ref any, $"+{bonusCoins} Coins / Win");
        }

        if (draftBonus != 0)
        {
            AppendEffect(builder, ref any, $"+{draftBonus} Draft Choice");
        }

        if (!Mathf.Approximately(blenderScale, 1f))
        {
            AppendEffect(
                builder,
                ref any,
                $"{Mathf.RoundToInt(blenderScale * 100f)}% Blender Width"
            );
        }

        if (!Mathf.Approximately(ingredientScale, 1f))
        {
            AppendEffect(
                builder,
                ref any,
                $"{Mathf.RoundToInt(ingredientScale * 100f)}% Ingredient Size"
            );
        }

        if (!any)
            builder.Append("No persistent effects yet");

        builder.AppendLine();
    }

    private void AppendEffect(
        StringBuilder builder,
        ref bool any,
        string value
    )
    {
        if (any)
            builder.Append("  •  ");

        builder.Append(value);
        any = true;
    }

    private void AppendPantry(StringBuilder builder)
    {
        int types = 0;
        int copies = 0;

        if (runProgress != null && runProgress.Pantry != null)
        {
            foreach (RunIngredientEntry entry in runProgress.Pantry)
            {
                if (entry == null || entry.Ingredient == null)
                    continue;

                types++;
                copies += entry.Copies;
            }
        }

        builder.AppendLine($"Pantry: {types} types  •  {copies} total copies");
    }

    private void AppendNextCustomer(StringBuilder builder)
    {
        if (runManager == null ||
            runManager.Definition == null ||
            runManager.Definition.customers == null ||
            runProgress == null)
        {
            return;
        }

        int nextDay = runProgress.Day + 1;
        int index = nextDay - 1;

        if (index < 0 || index >= runManager.Definition.customers.Count)
            return;

        CustomerDefinition nextCustomer =
            runManager.Definition.customers[index];

        if (nextCustomer == null)
            return;

        int goal =
            runManager.Definition.GetGoalScore(
                nextDay,
                nextCustomer
            );

        builder.AppendLine();
        builder.Append(
            $"NEXT: {nextCustomer.customerName.ToUpperInvariant()}  •  GOAL {goal}"
        );
    }

    private void BuildUI()
    {
        Transform existingCanvas =
            transform.Find("ShopBuildSummaryCanvas");

        if (existingCanvas != null)
        {
            ResolveExistingUI(existingCanvas);

            if (canvasGroup != null &&
                titleText != null &&
                bodyText != null &&
                rerollButton != null &&
                rerollButtonText != null)
            {
                return;
            }
        }

        GameObject canvasObject = new GameObject(
            "ShopBuildSummaryCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(CanvasGroup)
        );

        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1300;

        CanvasScaler scaler =
            canvasObject.GetComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1920f, 1080f);

        scaler.matchWidthOrHeight = 0.5f;

        canvasGroup =
            canvasObject.GetComponent<CanvasGroup>();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        RectTransform panel = CreateRect(
            "BuildSummaryPanel",
            canvasObject.transform,
            new Vector2(-26f, -26f),
            new Vector2(Width, Height)
        );

        panel.anchorMin = new Vector2(1f, 1f);
        panel.anchorMax = new Vector2(1f, 1f);
        panel.pivot = new Vector2(1f, 1f);

        Image background =
            panel.gameObject.AddComponent<Image>();

        background.color =
            new Color(0.07f, 0.045f, 0.025f, 0.92f);

        background.raycastTarget = false;

        titleText = CreateText(
            "BuildTitle",
            panel,
            new Vector2(18f, -16f),
            new Vector2(Width - 36f, 36f),
            25f,
            FontStyles.Bold
        );

        bodyText = CreateText(
            "BuildBody",
            panel,
            new Vector2(18f, -58f),
            new Vector2(Width - 36f, Height - 132f),
            20f,
            FontStyles.Normal
        );

        CreateRerollButton(panel);
    }

    private void ResolveExistingUI(Transform existingCanvas)
    {
        canvasGroup =
            existingCanvas.GetComponent<CanvasGroup>();

        if (existingCanvas.GetComponent<GraphicRaycaster>() == null)
            existingCanvas.gameObject.AddComponent<GraphicRaycaster>();

        Transform panel =
            existingCanvas.Find("BuildSummaryPanel");

        if (panel == null)
            return;

        RectTransform panelRect = panel as RectTransform;
        if (panelRect != null)
            panelRect.sizeDelta = new Vector2(Width, Height);

        Transform title = panel.Find("BuildTitle");
        Transform body = panel.Find("BuildBody");

        titleText = title != null
            ? title.GetComponent<TMP_Text>()
            : null;

        bodyText = body != null
            ? body.GetComponent<TMP_Text>()
            : null;

        if (body != null)
        {
            RectTransform bodyRect = body as RectTransform;
            if (bodyRect != null)
                bodyRect.sizeDelta = new Vector2(Width - 36f, Height - 132f);
        }

        Transform reroll = panel.Find("RerollButton");

        if (reroll == null)
        {
            CreateRerollButton(panel);
        }
        else
        {
            rerollButton = reroll.GetComponent<Button>();

            Transform label = reroll.Find("RerollLabel");
            rerollButtonText = label != null
                ? label.GetComponent<TMP_Text>()
                : null;
        }
    }

    private void BindRerollButton()
    {
        if (rerollButton == null || !Application.isPlaying)
            return;

        rerollButton.onClick.RemoveListener(HandleRerollPressed);
        rerollButton.onClick.AddListener(HandleRerollPressed);
    }

    private void HandleRerollPressed()
    {
        if (shopManager == null)
            return;

        shopManager.RerollOffers();
        RefreshRerollButton();
    }

    private void RefreshRerollButton()
    {
        if (rerollButton == null || rerollButtonText == null)
            return;

        if (shopManager == null || runProgress == null)
        {
            rerollButtonText.text = "REROLL UNAVAILABLE";
            rerollButton.interactable = false;
            return;
        }

        if (shopManager.HasOpenIngredientCrate)
        {
            rerollButtonText.text = "CHOOSE CRATE ITEM FIRST";
            rerollButton.interactable = false;
            return;
        }

        if (shopManager.RerollsRemaining <= 0)
        {
            rerollButtonText.text = "REROLL USED";
            rerollButton.interactable = false;
            return;
        }

        int cost = shopManager.RerollCost;

        if (runProgress.Coins < cost)
        {
            rerollButtonText.text = $"NEED ${cost} TO REROLL";
            rerollButton.interactable = false;
            return;
        }

        rerollButtonText.text = $"REROLL OFFERS  ${cost}";
        rerollButton.interactable = shopManager.CanRerollOffers();
    }

    private void CreateRerollButton(Transform panel)
    {
        RectTransform rect = CreateRect(
            "RerollButton",
            panel,
            new Vector2(18f, -302f),
            new Vector2(Width - 36f, 42f)
        );

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.92f, 0.34f, 0.08f, 1f);

        rerollButton = rect.gameObject.AddComponent<Button>();
        rerollButton.targetGraphic = image;

        rerollButtonText = CreateText(
            "RerollLabel",
            rect,
            Vector2.zero,
            rect.sizeDelta,
            18f,
            FontStyles.Bold
        );

        rerollButtonText.alignment = TextAlignmentOptions.Center;
        BindRerollButton();
    }

    private RectTransform CreateRect(
        string objectName,
        Transform parent,
        Vector2 position,
        Vector2 size
    )
    {
        GameObject obj =
            new GameObject(
                objectName,
                typeof(RectTransform)
            );

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
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
        RectTransform rect =
            CreateRect(
                objectName,
                parent,
                position,
                size
            );

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);

        TextMeshProUGUI text =
            rect.gameObject.AddComponent<TextMeshProUGUI>();

        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color =
            new Color(1f, 0.94f, 0.82f, 1f);

        text.alignment = TextAlignmentOptions.TopLeft;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;

        return text;
    }
}
