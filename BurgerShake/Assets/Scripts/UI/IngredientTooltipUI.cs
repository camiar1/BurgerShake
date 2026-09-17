using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class IngredientTooltipUI : MonoBehaviour
{
    private Canvas canvas;
    private RectTransform canvasRect;
    private RectTransform panel;
    private TMP_Text bodyText;
    private CanvasGroup canvasGroup;

    private readonly Vector2 cursorOffset = new Vector2(22f, -18f);
    private const float Width = 430f;

    private void Awake()
    {
        BuildUI();
        Hide();
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
        bodyText.text = BuildTooltipText(ingredient);
        bodyText.ForceMeshUpdate();

        float height = Mathf.Clamp(bodyText.preferredHeight + 34f, 120f, 420f);
        panel.sizeDelta = new Vector2(Width, height);

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

    private string BuildTooltipText(Ingredient ingredient)
    {
        IngredientDefinition definition = ingredient.Definition;
        StringBuilder builder = new StringBuilder();

        builder.Append("<size=30><b>");
        builder.Append(definition.ingredientName);
        builder.AppendLine("</b></size>");
        builder.Append("<b>TYPE:</b> ");
        builder.AppendLine(GetPrimaryType(definition));
        builder.Append("<b>TOUCHING:</b> ");
        builder.AppendLine(ingredient.TouchingCount.ToString());

        if (!string.IsNullOrWhiteSpace(definition.description))
        {
            builder.AppendLine();
            builder.AppendLine(definition.description.Trim());
        }

        builder.AppendLine();
        builder.AppendLine("<b>SCORING</b>");

        if (definition.scoringRules == null || definition.scoringRules.Count == 0)
        {
            builder.Append("• No special scoring rule");
            return builder.ToString();
        }
        foreach (IngredientScoringRule rule in definition.scoringRules)
        {
            if (rule == null)
                continue;

            builder.Append("• ");
            builder.AppendLine(GetRuleText(rule));
        }

        builder.AppendLine();
        builder.Append("<size=18><color=#C8C8C8>Tags: ");
        builder.Append(string.Join(", ", definition.tags));
        builder.Append("</color></size>");
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
        panel.anchorMin = new Vector2(0f, 1f);
        panel.anchorMax = new Vector2(0f, 1f);
        panel.pivot = new Vector2(0f, 1f);
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
        bodyText.enableWordWrapping = true;
        bodyText.raycastTarget = false;
        bodyText.richText = true;
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

        Vector2 pos = localPoint + cursorOffset;
        Rect bounds = canvasRect.rect;
        float width = panel.rect.width;
        float height = panel.rect.height;

        if (pos.x + width > bounds.xMax) pos.x = bounds.xMax - width - 10f;
        if (pos.y - height < bounds.yMin) pos.y = bounds.yMin + height + 10f;

        panel.anchoredPosition = pos;
    }
}
