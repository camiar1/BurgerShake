using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// New, self-contained draft presentation. Does not restyle any existing scene artwork.
public class StartingDraftSelectionView : MonoBehaviour
{
    private sealed class Card
    {
        public RectTransform root;
        public Button choose;
        public Image art;
        public Image halo;
        public TMP_Text type;
        public TMP_Text name;
        public TMP_Text description;
        public TMP_Text tags;
        public Button reroll;
        public TMP_Text rerollLabel;
    }

    private readonly List<Card> cards = new List<Card>();
    private readonly List<GameObject> pickedSlots = new List<GameObject>();
    private StartingIngredientDraftController controller;
    private Canvas canvas;
    private CanvasGroup group;
    private RectTransform layout;
    private TMP_Text progressText;
    private TMP_Text instructionText;
    private TMP_Text rerollsText;
    private RectTransform pickedRow;
    private int displayedRound;
    private float chooseReadyAt;
    private static Sprite haloSprite;

    private const float CardWidth = 340f;
    private const float CardHeight = 570f;
    private const float CardGap = 32f;
    private static readonly Color Gold = new Color32(217, 166, 83, 255);
    private static readonly Color Cream = new Color32(255, 240, 207, 255);

    private void LateUpdate() { RefreshLayout(); }

    public void RefreshLayout()
    {
        if (canvas == null || layout == null) return;
        Rect available = ((RectTransform)canvas.transform).rect;
        if (available.width <= 0f || available.height <= 0f) return;
        float fit = Mathf.Min(1f, (available.width - 40f) / 1320f, (available.height - 40f) / 980f);
        layout.localScale = Vector3.one * Mathf.Max(0.1f, fit);
    }

    public void Present(StartingIngredientDraftController draft)
    {
        controller = draft;
        if (draft == null || !draft.DraftActive) { Hide(); return; }
        bool newRound = displayedRound != draft.CurrentRound || !gameObject.activeSelf;
        Build();
        gameObject.SetActive(true);
        group.alpha = 1f;
        group.blocksRaycasts = true;
        group.interactable = true;
        progressText.text = $"PICK {draft.CurrentRound} OF {draft.RoundCount}";
        instructionText.text = draft.CurrentRound == 1
            ? "Choose an ingredient to start your build."
            : "Choose your next ingredient. Make the combo yours.";
        rerollsText.text = draft.RerollsRemaining > 0
            ? $"{draft.RerollsRemaining} REROLL{(draft.RerollsRemaining == 1 ? "" : "S")} LEFT  /  shared across all cards"
            : "NO REROLLS LEFT";
        FillCards(draft.CurrentChoices, draft.CopiesPerPick, draft.RerollsRemaining > 0);
        FillPicked(draft.SelectedIngredients, draft.RoundCount);
        if (newRound)
        {
            // A double-click must not accidentally spend both starting picks.
            chooseReadyAt = Time.unscaledTime + 0.2f;
            displayedRound = draft.CurrentRound;
            if (Application.isPlaying && EventSystem.current != null && cards.Count > 0)
                EventSystem.current.SetSelectedGameObject(cards[0].choose.gameObject);
        }
    }

    public void Hide()
    {
        if (group != null) { group.alpha = 0f; group.blocksRaycasts = false; group.interactable = false; }
        displayedRound = 0;
        gameObject.SetActive(false);
    }

    // Used by the isolated editor review without touching a live run.
    public void ShowPreview(IReadOnlyList<IngredientDefinition> choices, IngredientDefinition firstPick = null)
    {
        controller = null;
        Build();
        gameObject.SetActive(true);
        group.alpha = 1f;
        group.blocksRaycasts = false;
        group.interactable = true;
        progressText.text = firstPick == null ? "PICK 1 OF 2" : "PICK 2 OF 2";
        instructionText.text = firstPick == null ? "Choose an ingredient to start your build." : "Choose your next ingredient. Make the combo yours.";
        rerollsText.text = "1 REROLL LEFT  /  shared across all cards";
        FillCards(choices, 3, true);
        FillPicked(firstPick == null ? new List<IngredientDefinition>() : new List<IngredientDefinition> { firstPick }, 2);
    }

    private void Build()
    {
        if (canvas != null) return;
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1700;
        gameObject.AddComponent<GraphicRaycaster>();
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        group = gameObject.AddComponent<CanvasGroup>();

        RectTransform backdrop = Rect("DraftBackdrop", transform, Vector2.zero, Vector2.zero);
        Stretch(backdrop);
        Image dim = backdrop.gameObject.AddComponent<Image>();
        dim.color = new Color(0.055f, 0.035f, 0.025f, 0.97f);
        dim.raycastTarget = true;

        layout = Rect("DraftLayout", transform, Vector2.zero, new Vector2(1320f, 960f));
        TMP_Text eyebrow = Label("Eyebrow", layout, "STARTING INGREDIENTS", new Vector2(0f, 444f), new Vector2(800f, 28f), 17f);
        eyebrow.color = Gold;
        TMP_Text title = Label("DraftTitle", layout, "STOCK YOUR PANTRY", new Vector2(0f, 394f), new Vector2(1100f, 70f), 52f, true);
        title.color = Cream;
        progressText = Label("DraftProgress", layout, "PICK 1 OF 2", new Vector2(0f, 338f), new Vector2(400f, 32f), 22f, true);
        progressText.color = Gold;
        instructionText = Label("DraftInstruction", layout, "Choose an ingredient to start your build.", new Vector2(0f, 298f), new Vector2(1100f, 34f), 21f);
        instructionText.color = new Color32(216, 194, 161, 255);

        rerollsText = Label("RerollsRemaining", layout, "", new Vector2(0f, -397f), new Vector2(1000f, 28f), 17f);
        rerollsText.color = new Color32(216, 194, 161, 255);
        pickedRow = Rect("PickedIngredients", layout, new Vector2(0f, -454f), new Vector2(1000f, 66f));
    }

    private void FillCards(IReadOnlyList<IngredientDefinition> choices, int copies, bool canReroll)
    {
        int count = choices != null ? choices.Count : 0;
        while (cards.Count < count) cards.Add(CreateCard(cards.Count));
        float totalWidth = count * CardWidth + Mathf.Max(0, count - 1) * CardGap;
        // The game currently offers 3. Keep a larger configured offer count on screen too.
        float scale = Mathf.Min(1f, 1230f / Mathf.Max(1f, totalWidth));
        for (int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            IngredientDefinition ingredient = i < count ? choices[i] : null;
            card.root.gameObject.SetActive(ingredient != null);
            card.reroll.gameObject.SetActive(ingredient != null);
            if (ingredient == null) continue;
            Vector2 position = new Vector2((i * (CardWidth + CardGap) - (totalWidth - CardWidth) * 0.5f) * scale, -26f);
            card.root.anchoredPosition = position;
            card.root.localScale = Vector3.one * scale;
            card.root.GetComponent<StartingDraftCardHover>().SetBase(position, scale);
            card.art.sprite = DisplaySprite(ingredient);
            card.art.enabled = card.art.sprite != null;
            FitIngredientArt(card.art);
            card.name.text = ingredient.ingredientName.ToUpperInvariant();
            card.type.text = PrimaryType(ingredient).ToUpperInvariant();
            card.description.text = ingredient.description;
            card.tags.text = string.Join("  /  ", ingredient.tags);
            card.halo.color = ingredient.HasTag(IngredientTag.Fruit) ? new Color32(225, 153, 86, 100)
                : ingredient.HasTag(IngredientTag.Protein) ? new Color32(219, 177, 118, 100)
                : new Color32(169, 186, 106, 100);
            card.choose.interactable = true;
            card.reroll.interactable = canReroll;
            card.rerollLabel.text = canReroll ? "REROLL THIS CARD" : "NO REROLLS LEFT";
            card.root.Find("Copies").GetComponent<TMP_Text>().text = $"ADD {copies} COPIES";
        }
    }

    private Card CreateCard(int index)
    {
        var card = new Card();
        card.root = Rect("IngredientCard" + index, layout, Vector2.zero, new Vector2(CardWidth, CardHeight));
        Image face = card.root.gameObject.AddComponent<Image>();
        BurgerShakeGeneratedUITheme.Surface(face);
        card.choose = card.root.gameObject.AddComponent<Button>();
        card.choose.targetGraphic = face;
        face.raycastTarget = true;
        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.highlightedColor = new Color(1f, 0.94f, 0.78f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.pressedColor = new Color(0.86f, 0.76f, 0.58f, 1f);
        colors.fadeDuration = 0.12f;
        card.choose.colors = colors;
        card.choose.onClick.AddListener(() =>
        {
            if (controller != null && (!Application.isPlaying || Time.unscaledTime >= chooseReadyAt)) controller.Choose(index);
        });

        RectTransform artField = Rect("ArtField", card.root, new Vector2(0f, 116f), new Vector2(316f, 312f));
        Image artBackground = artField.gameObject.AddComponent<Image>();
        artBackground.color = new Color32(66, 40, 24, 255);
        artBackground.raycastTarget = false;
        card.halo = Rect("IngredientHalo", artField, new Vector2(0f, -12f), new Vector2(286f, 286f)).gameObject.AddComponent<Image>();
        card.halo.sprite = HaloSprite();
        card.halo.raycastTarget = false;
        card.art = Rect("LargeIngredientSprite", artField, new Vector2(0f, -12f), new Vector2(274f, 250f)).gameObject.AddComponent<Image>();
        card.art.preserveAspect = true;
        card.art.useSpriteMesh = true;
        card.art.raycastTarget = false;
        card.type = Label("IngredientType", artField, "FRUIT", new Vector2(0f, 129f), new Vector2(280f, 26f), 15f);
        card.type.color = Gold;
        card.name = Label("IngredientName", card.root, "INGREDIENT", new Vector2(0f, -72f), new Vector2(292f, 54f), 30f, true);
        card.name.enableAutoSizing = true;
        card.name.fontSizeMin = 23f;
        card.name.fontSizeMax = 30f;
        card.name.textWrappingMode = TextWrappingModes.NoWrap;
        card.description = Label("IngredientEffect", card.root, "", new Vector2(0f, -148f), new Vector2(282f, 98f), 20f);
        card.description.enableAutoSizing = true;
        card.description.fontSizeMin = 16f;
        card.description.fontSizeMax = 20f;
        card.description.alignment = TextAlignmentOptions.Top;
        card.tags = Label("IngredientTags", card.root, "", new Vector2(0f, -218f), new Vector2(286f, 28f), 14f);
        card.tags.color = BurgerShakeGeneratedUITheme.MutedInk;
        Label("Copies", card.root, "ADD 3 COPIES", new Vector2(0f, -258f), new Vector2(260f, 26f), 17f, true);

        RectTransform rerollRect = Rect("RerollCard" + index, layout, Vector2.zero, new Vector2(268f, 44f));
        Image rerollImage = rerollRect.gameObject.AddComponent<Image>();
        card.reroll = rerollRect.gameObject.AddComponent<Button>();
        card.rerollLabel = Label("RerollLabel", rerollRect, "REROLL THIS CARD", Vector2.zero, new Vector2(244f, 32f), 18f, true);
        BurgerShakeGeneratedUITheme.Button(card.reroll, card.rerollLabel, false);
        card.reroll.onClick.AddListener(() => controller?.RerollChoice(index));
        StartingDraftCardHover hover = card.root.gameObject.AddComponent<StartingDraftCardHover>();
        hover.Configure(card.choose, artBackground, rerollRect);
        return card;
    }

    private void FillPicked(IReadOnlyList<IngredientDefinition> picked, int total)
    {
        while (pickedSlots.Count < total)
        {
            RectTransform slot = Rect("Pick" + pickedSlots.Count, pickedRow, Vector2.zero, new Vector2(260f, 60f));
            Image background = slot.gameObject.AddComponent<Image>();
            BurgerShakeGeneratedUITheme.Surface(background, false, false);
            Image icon = Rect("Icon", slot, new Vector2(-98f, 0f), new Vector2(46f, 46f)).gameObject.AddComponent<Image>();
            icon.preserveAspect = true;
            icon.useSpriteMesh = true;
            icon.raycastTarget = false;
            Label("PickLabel", slot, "", new Vector2(24f, 0f), new Vector2(188f, 42f), 20f, true);
            pickedSlots.Add(slot.gameObject);
        }
        for (int i = 0; i < pickedSlots.Count; i++)
        {
            GameObject slot = pickedSlots[i];
            slot.SetActive(i < total);
            if (i >= total) continue;
            ((RectTransform)slot.transform).anchoredPosition = new Vector2((i - (total - 1) * 0.5f) * 280f, 0f);
            IngredientDefinition ingredient = picked != null && i < picked.Count ? picked[i] : null;
            Image icon = slot.transform.Find("Icon").GetComponent<Image>();
            icon.sprite = ingredient != null ? DisplaySprite(ingredient) : null;
            icon.enabled = icon.sprite != null;
            TMP_Text label = slot.transform.Find("PickLabel").GetComponent<TMP_Text>();
            label.text = ingredient != null ? ingredient.ingredientName.ToUpperInvariant() : $"PICK {i + 1}";
            label.enableAutoSizing = true;
            label.fontSizeMin = 14f;
            label.fontSizeMax = 20f;
            label.color = ingredient != null ? BurgerShakeGeneratedUITheme.Ink : BurgerShakeGeneratedUITheme.MutedInk;
        }
    }

    public static Sprite DisplaySprite(IngredientDefinition ingredient)
    {
        if (ingredient == null) return null;
        if (ingredient.sprite != null) return ingredient.sprite;
        // Several final-art fruit definitions currently have no UI sprite assignment.
        // Read their prefab art without changing the user's definition or prefab.
        if (ingredient.prefab != null)
        {
            SpriteRenderer renderer = ingredient.prefab.GetComponentInChildren<SpriteRenderer>(true);
            if (renderer != null && renderer.sprite != null) return renderer.sprite;
        }
        return ingredient.GetDisplaySprite();
    }

    private static void FitIngredientArt(Image image)
    {
        Vector2 home = new Vector2(0f, -12f);
        Vector2 box = new Vector2(274f, 250f);
        image.rectTransform.anchoredPosition = home;
        image.rectTransform.sizeDelta = box;
        if (image.sprite == null) return;
        // Tight sprite meshes can be much smaller than their transparent source rect.
        // Fit the visible mesh, so a padded nugget is as prominent as a cropped fruit.
        Vector2[] vertices = image.sprite.vertices;
        if (vertices.Length < 3) return;
        Vector2 min = vertices[0], max = min;
        foreach (Vector2 vertex in vertices) { min = Vector2.Min(min, vertex); max = Vector2.Max(max, vertex); }
        Vector2 visible = max - min;
        if (visible.x <= 0.0001f || visible.y <= 0.0001f) return;
        float scale = Mathf.Min(box.x / visible.x, box.y / visible.y);
        image.rectTransform.sizeDelta = (Vector2)image.sprite.bounds.size * scale;
        image.rectTransform.anchoredPosition = home - ((min + max) * 0.5f - (Vector2)image.sprite.bounds.center) * scale;
    }

    private static string PrimaryType(IngredientDefinition ingredient) => ingredient.HasTag(IngredientTag.Fruit) ? "Fruit"
        : ingredient.HasTag(IngredientTag.Protein) ? "Protein" : "Vegetable";

    private static TMP_Text Label(string name, Transform parent, string value, Vector2 position, Vector2 size, float fontSize, bool heading = false)
    {
        RectTransform rect = Rect(name, parent, position, size);
        TMP_Text text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        BurgerShakeGeneratedUITheme.Text(text, fontSize, heading);
        text.text = value;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        return text;
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static Sprite HaloSprite()
    {
        if (haloSprite != null) return haloSprite;
        const int size = 128;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "BS Draft Ingredient Halo";
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.wrapMode = TextureWrapMode.Clamp;
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float r = Vector2.Distance(new Vector2(x, y), new Vector2(63.5f, 63.5f)) / 63.5f;
            float alpha = Mathf.Pow(Mathf.Clamp01(1f - r), 1.4f);
            pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
        }
        texture.SetPixels(pixels);
        texture.Apply(false, true);
        haloSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        haloSprite.hideFlags = HideFlags.HideAndDontSave;
        return haloSprite;
    }
}

public class StartingDraftCardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private Button button;
    private Image artField;
    private RectTransform reroll;
    private RectTransform rect;
    private Vector2 position;
    private float scale = 1f;
    private bool hovered;
    private bool selected;

    public void Configure(Button chooseButton, Image artBackground, RectTransform rerollButton)
    {
        button = chooseButton;
        artField = artBackground;
        reroll = rerollButton;
        rect = (RectTransform)transform;
    }

    public void SetBase(Vector2 basePosition, float baseScale)
    {
        position = basePosition;
        scale = baseScale;
        rect.anchoredPosition = position;
        rect.localScale = Vector3.one * scale;
        reroll.anchoredPosition = new Vector2(position.x, position.y - 320f * scale);
        reroll.localScale = Vector3.one * scale;
    }

    private void Update()
    {
        if (rect == null) return;
        bool active = (hovered || selected) && button.interactable;
        float t = 1f - Mathf.Exp(-18f * Time.unscaledDeltaTime);
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, position + Vector2.up * (active ? 10f : 0f), t);
        rect.localScale = Vector3.Lerp(rect.localScale, Vector3.one * scale * (active ? 1.025f : 1f), t);
        artField.color = Color.Lerp(artField.color, active ? new Color32(94, 59, 30, 255) : new Color32(66, 40, 24, 255), t);
    }

    private void OnDisable() { hovered = selected = false; }
    public void OnPointerEnter(PointerEventData data) { hovered = true; }
    public void OnPointerExit(PointerEventData data) { hovered = false; }
    public void OnSelect(BaseEventData data) { selected = true; }
    public void OnDeselect(BaseEventData data) { selected = false; }
}
