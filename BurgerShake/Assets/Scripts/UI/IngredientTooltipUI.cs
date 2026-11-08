using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[ExecuteAlways]
public class IngredientTooltipUI : MonoBehaviour
{
    private Canvas canvas;
    private RectTransform canvasRect;
    private RectTransform panel;
    private TMP_Text bodyText;
    private TMP_Text titleText;
    private TMP_Text metaText;
    private TMP_Text traitsText;
    private CanvasGroup canvasGroup;

    private const float CursorGap = 14f;
    private const float ScreenPadding = 10f;
    private const float Width = 420f;

    private void Awake()
    {
        BuildUI();

        if (Application.isPlaying)
            Hide();
        else
            ShowEditorPreview();
    }

    private void OnEnable()
    {
        BuildUI();

        if (!Application.isPlaying)
            ShowEditorPreview();
    }

    private void LateUpdate()
    {
        if (canvasGroup == null || canvasGroup.alpha <= 0f || Mouse.current == null)
            return;

        PositionNearCursor(Mouse.current.position.ReadValue());
    }

    public void Show(Ingredient ingredient)
    {
        if (ingredient == null || ingredient.Definition == null)
        {
            Hide();
            return;
        }

        ShowDefinition(ingredient.Definition, ingredient.TouchingCount);
    }

    public void Show(IngredientDefinition definition)
    {
        if (definition == null)
        {
            Hide();
            return;
        }

        ShowDefinition(definition, null);
    }

    private void ShowDefinition(IngredientDefinition definition, int? touchingCount)
    {
        BuildUI();

        if (bodyText == null || panel == null || canvasGroup == null)
            return;

        titleText.text = definition.ingredientName.ToUpperInvariant();
        metaText.text = GetPrimaryType(definition).ToUpperInvariant() +
            (touchingCount.HasValue ? $"   /   {touchingCount.Value} CONTACTS" : "");
        traitsText.text = "TRAITS   " + string.Join("  /  ", definition.tags);
        bodyText.text = BuildTooltipText(definition, touchingCount);
        FitContent();

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void Hide()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    private string BuildTooltipText(IngredientDefinition definition, int? touchingCount)
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine("<size=15><b>SCORING</b></size>");

        if (definition.scoringRules == null || definition.scoringRules.Count == 0)
        {
            builder.Append(string.IsNullOrWhiteSpace(definition.description) ? "No special scoring rule" : definition.description.Trim());
            return builder.ToString();
        }
        foreach (IngredientScoringRule rule in definition.scoringRules)
        {
            if (rule == null)
                continue;

            builder.Append("• ");
            builder.AppendLine(GetRuleText(rule));
        }

        return builder.ToString();
    }

    private string GetPrimaryType(IngredientDefinition definition)
    {
        if (definition.HasTag(IngredientTag.Fruit)) return "Fruit";
        if (definition.HasTag(IngredientTag.Protein)) return "Protein";
        if (definition.HasTag(IngredientTag.Vegetable)) return "Vegetable";
        return "Special";
    }

    private string GetRuleText(IngredientScoringRule rule)
    {
        if (!string.IsNullOrWhiteSpace(rule.description))
            return rule.description.Trim();

        string reward = rule.reward == ScoringReward.Points
            ? $"+{rule.amount:0.##} Points"
            : $"+{rule.amount:0.##} Mult";

        if (rule is ContactCountScoringRule contacts)
            return $"Touching {contacts.minimumContacts}+ ingredients → {reward}";

        if (rule is OrientationScoringRule orientation)
            return $"{orientation.orientation} within {orientation.angleTolerance:0}° → {reward}";

        if (rule is CrossTagScoringRule cross)
            return $"Touch both {cross.firstTag} and {cross.secondTag} → {reward}";

        if (rule is UniqueNeighborScoringRule unique)
            return $"{unique.minimumUniqueNeighbors}+ unique neighbors → {reward}";

        if (rule is ClusterScoringRule cluster)
            return $"Cluster of {cluster.minimumClusterSize}+ matching ingredients → {reward}";
        if (rule is BridgeScoringRule)
            return $"Bridge two ingredients that are not touching → {reward}";

        if (rule is VerticalRelationshipScoringRule vertical)
            return $"{vertical.relationship} → {reward}";

        if (rule is TagDiversityScoringRule diversity)
            return $"Touch {diversity.minimumDifferentTags}+ ingredient categories → {reward}";

        switch (rule.target)
        {
            case ScoringTarget.TouchingAny:
                return $"Per touching ingredient → {reward}";
            case ScoringTarget.TouchingTag:
                return $"Per touching {rule.requiredTag} → {reward}";
            case ScoringTarget.TouchingIngredient:
                string targetName = rule.requiredIngredient != null
                    ? rule.requiredIngredient.ingredientName
                    : "specific ingredient";
                return $"Per touching {targetName} → {reward}";
            default:
                return reward;
        }
    }

    private void BuildUI()
    {
        Transform existingCanvas = transform.Find("IngredientTooltipCanvas");

        if (existingCanvas != null)
        {
            ResolveExistingUI(existingCanvas);

            if (canvas != null && panel != null && bodyText != null && canvasGroup != null)
            {
                ApplyTheme();
                return;
            }
        }

        GameObject canvasObject = new GameObject(
            "IngredientTooltipCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(CanvasGroup)
        );
        canvasObject.transform.SetParent(transform, false);

        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 3000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGroup = canvasObject.GetComponent<CanvasGroup>();
        canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.anchorMin = Vector2.zero;
        canvasRect.anchorMax = Vector2.one;
        canvasRect.sizeDelta = Vector2.zero;

        GameObject panelObject = new GameObject("TooltipPanel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);
        panel = panelObject.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 1f);
        panel.sizeDelta = new Vector2(Width, 180f);

        Image background = panelObject.GetComponent<Image>();
        background.color = new Color(0.07f, 0.045f, 0.025f, 0.96f);
        background.raycastTarget = false;

        GameObject textObject = new GameObject("TooltipText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panelObject.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(16f, 14f);
        textRect.offsetMax = new Vector2(-16f, -14f);

        bodyText = textObject.GetComponent<TextMeshProUGUI>();
        bodyText.fontSize = 21f;
        bodyText.color = new Color(1f, 0.94f, 0.82f, 1f);
        bodyText.alignment = TextAlignmentOptions.TopLeft;
        bodyText.textWrappingMode = TextWrappingModes.Normal;
        bodyText.raycastTarget = false;
        bodyText.richText = true;
        ApplyTheme();
    }

    private void ResolveExistingUI(Transform existingCanvas)
    {
        canvas = existingCanvas.GetComponent<Canvas>();
        canvasRect = existingCanvas as RectTransform;
        canvasGroup = existingCanvas.GetComponent<CanvasGroup>();

        Transform panelTransform = existingCanvas.Find("TooltipPanel");
        panel = panelTransform as RectTransform;

        if (panel != null)
        {
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
        }

        if (panelTransform != null)
        {
            Transform textTransform = panelTransform.Find("TooltipText");
            bodyText = textTransform != null
                ? textTransform.GetComponent<TMP_Text>()
                : null;
        }
    }

    private void ShowEditorPreview()
    {
        if (bodyText == null || panel == null || canvasGroup == null)
            return;

        titleText.text = "APPLE";
        metaText.text = "FRUIT   /   2 CONTACTS";
        traitsText.text = "TRAITS   Fruit  /  Round";
        bodyText.text = "<size=15><b>SCORING</b></size>\n• Base +2 Points.\n• +0.10 Mult per unique neighboring ingredient type.";
        FitContent();
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 1f);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        panel.anchoredPosition = new Vector2(0f, 170f);
    }

    private void ApplyTheme()
    {
        BurgerShakeGeneratedUITheme.Surface(panel.GetComponent<Image>());
        titleText = BurgerShakeGeneratedUITheme.Label(panel, "BS_TooltipTitle");
        metaText = BurgerShakeGeneratedUITheme.Label(panel, "BS_TooltipMeta");
        traitsText = BurgerShakeGeneratedUITheme.Label(panel, "BS_TooltipTraits");
        BurgerShakeGeneratedUITheme.Text(titleText, 30f, true);
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = 21f;
        titleText.fontSizeMax = 30f;
        titleText.textWrappingMode = TextWrappingModes.NoWrap;
        BurgerShakeGeneratedUITheme.Text(metaText, 15f, false, true);
        BurgerShakeGeneratedUITheme.Text(traitsText, 15f, false, true);
        BurgerShakeGeneratedUITheme.Text(bodyText, 20f);
        bodyText.textWrappingMode = TextWrappingModes.Normal;
        bodyText.enableAutoSizing = true;
        bodyText.fontSizeMin = 15f;
        bodyText.fontSizeMax = 20f;
        BurgerShakeGeneratedUITheme.Divider(panel, "BS_TooltipDivider", 82f, 24f);
    }

    private void FitContent()
    {
        float width = canvasRect != null && canvasRect.rect.width > 0f
            ? Mathf.Min(Width, canvasRect.rect.width - ScreenPadding * 2f) : Width;
        float contentWidth = Mathf.Max(80f, width - 48f);
        // Measure at the normal body size, then allow auto-sizing on short screens.
        bodyText.enableAutoSizing = false;
        bodyText.fontSize = 20f;
        float bodyHeight = bodyText.GetPreferredValues(bodyText.text, contentWidth, Mathf.Infinity).y + 8f;
        float footerHeight = traitsText.GetPreferredValues(traitsText.text, contentWidth, Mathf.Infinity).y + 4f;
        float maxHeight = canvasRect != null && canvasRect.rect.height > 0f
            ? canvasRect.rect.height - ScreenPadding * 2f : 800f;
        float height = Mathf.Min(Mathf.Max(208f, 100f + bodyHeight + footerHeight + 36f), maxHeight);
        panel.sizeDelta = new Vector2(width, height);
        BurgerShakeGeneratedUITheme.TopLeft(titleText.rectTransform, 24f, 18f, contentWidth, 38f);
        BurgerShakeGeneratedUITheme.TopLeft(metaText.rectTransform, 24f, 58f, contentWidth, 22f);
        BurgerShakeGeneratedUITheme.TopLeft(bodyText.rectTransform, 24f, 98f, contentWidth,
            Mathf.Max(30f, height - 98f - footerHeight - 36f));
        BurgerShakeGeneratedUITheme.TopLeft(traitsText.rectTransform, 24f, height - footerHeight - 20f, contentWidth, footerHeight);
        bodyText.enableAutoSizing = true;
    }

    private void PositionNearCursor(Vector2 screenPosition)
    {
        if (canvasRect == null || panel == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            null,
            out Vector2 localPoint
        );

        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);

        bool cursorInTopHalf =
            screenPosition.y >= Screen.height * 0.5f;

        panel.pivot = cursorInTopHalf
            ? new Vector2(0.5f, 1f)
            : new Vector2(0.5f, 0f);

        Vector2 pos =
            localPoint +
            Vector2.up * (cursorInTopHalf ? -CursorGap : CursorGap);

        Rect bounds = canvasRect.rect;
        float halfWidth = panel.rect.width * 0.5f;
        float height = panel.rect.height;

        pos.x = Mathf.Clamp(
            pos.x,
            bounds.xMin + halfWidth + ScreenPadding,
            bounds.xMax - halfWidth - ScreenPadding
        );

        if (cursorInTopHalf)
        {
            pos.y = Mathf.Clamp(
                pos.y,
                bounds.yMin + height + ScreenPadding,
                bounds.yMax - ScreenPadding
            );
        }
        else
        {
            pos.y = Mathf.Clamp(
                pos.y,
                bounds.yMin + ScreenPadding,
                bounds.yMax - height - ScreenPadding
            );
        }

        panel.anchoredPosition = pos;
    }
}
