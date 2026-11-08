using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Opt-in styling for the assistant-built panels only. Never scans/restyles scene UI.
public static class BurgerShakeGeneratedUITheme
{
    public static readonly Color Ink = new Color32(59, 34, 20, 255);
    public static readonly Color MutedInk = new Color32(125, 88, 55, 255);
    public static readonly Color Paper = new Color32(255, 240, 207, 255);
    public static readonly Color Accent = new Color32(153, 65, 34, 255);
    public static readonly Color Rule = new Color32(199, 161, 108, 255);
    private static Sprite paperSprite;
    private static Sprite primarySprite;
    private static TMP_FontAsset headingFont;
    private static Material headingMaterial;

    public static TMP_FontAsset HeadingFont
    {
        get
        {
            if (headingFont != null) return headingFont;
#if UNITY_EDITOR
            headingFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BarberChop SDF.asset");
#endif
            if (headingFont == null)
                foreach (TMP_FontAsset font in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
                    if (font.name == "BarberChop SDF") { headingFont = font; break; }
            return headingFont != null ? headingFont : TMP_Settings.defaultFontAsset;
        }
    }

    public static void Surface(Image image, bool primary = false, bool shadow = true)
    {
        if (image == null) return;
        image.sprite = SurfaceSprite(primary);
        image.type = Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = 1f;
        image.color = Color.white;
        image.raycastTarget = image.GetComponent<Button>() != null;
        // One subtle offset shadow, rather than a large fuzzy glow.
        Shadow effect = image.GetComponent<Shadow>();
        if (shadow && effect == null) effect = image.gameObject.AddComponent<Shadow>();
        if (effect != null)
        {
            effect.enabled = shadow;
            effect.effectColor = new Color(0.12f, 0.065f, 0.025f, 0.24f);
            effect.effectDistance = new Vector2(0f, -4f);
            effect.useGraphicAlpha = true;
        }
    }

    public static void Text(TMP_Text text, float size, bool heading = false, bool muted = false)
    {
        if (text == null) return;
        TMP_FontAsset font = heading ? HeadingFont : TMP_Settings.defaultFontAsset;
        if (font != null) text.font = font;
        // The supplied BarberChop atlas has zero padding and a gradient scale of 1.
        // Give only OUR labels a bitmap material; do not edit its shared font asset.
        if (heading && font != null && font.atlasPadding == 0)
        {
            if (headingMaterial == null)
            {
                Shader shader = Shader.Find("TextMeshPro/Mobile/Bitmap");
                if (shader == null) shader = Shader.Find("UI/Default");
                if (shader != null)
                {
                    headingMaterial = new Material(shader);
                    headingMaterial.name = "BS Generated Heading Material";
                    headingMaterial.hideFlags = HideFlags.HideAndDontSave;
                    headingMaterial.mainTexture = font.atlasTexture;
                }
            }
            if (headingMaterial != null) text.fontSharedMaterial = headingMaterial;
        }
        text.fontSize = size;
        text.color = muted ? MutedInk : Ink;
        text.fontStyle = heading ? FontStyles.Bold : FontStyles.Normal;
        text.raycastTarget = false;
        text.richText = true;
        text.lineSpacing = 4f;
        text.paragraphSpacing = 5f;
        text.overflowMode = TextOverflowModes.Ellipsis;
    }

    public static void Button(Button button, TMP_Text label, bool primary)
    {
        if (button == null) return;
        Image image = button.GetComponent<Image>();
        Surface(image, primary);
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.95f, 0.83f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.pressedColor = new Color(0.80f, 0.70f, 0.55f, 1f);
        colors.disabledColor = new Color(0.78f, 0.75f, 0.68f, 0.65f);
        colors.fadeDuration = 0.12f;
        button.colors = colors;
        Text(label, 23f, true);
        if (label != null)
        {
            label.color = primary ? Paper : Ink;
            label.alignment = TextAlignmentOptions.Center;
            label.enableAutoSizing = true;
            label.fontSizeMin = 15f;
            label.fontSizeMax = 23f;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            RectTransform rect = label.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(16f, 6f);
            rect.offsetMax = new Vector2(-16f, -6f);
        }
    }

    public static TMP_Text Label(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing.GetComponent<TMP_Text>();
        var obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        return obj.GetComponent<TMP_Text>();
    }

    public static void TopLeft(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
    }

    public static void Divider(Transform parent, string name, float top, float margin)
    {
        Transform existing = parent.Find(name);
        Image image;
        if (existing == null)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            image = obj.GetComponent<Image>();
        }
        else image = existing.GetComponent<Image>();
        RectTransform rect = image.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(margin, -top - 1f);
        rect.offsetMax = new Vector2(-margin, -top);
        image.color = Rule;
        image.raycastTarget = false;
    }

    // New, independently generated nine-slice art. No supplied PNG/sprite is modified.
    private static Sprite SurfaceSprite(bool primary)
    {
        Sprite cached = primary ? primarySprite : paperSprite;
        if (cached != null) return cached;
        const int size = 64;
        const float radius = 10f;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = primary ? "BS Generated Terracotta Frame" : "BS Generated Paper Frame";
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
            Vector2 nearest = new Vector2(Mathf.Clamp(point.x, radius, size - radius), Mathf.Clamp(point.y, radius, size - radius));
            float edge = radius - Vector2.Distance(point, nearest);
            Color color;
            if (edge < 0f) color = Color.clear;
            else if (edge < 3f) color = Ink;
            else if (edge < 4.5f) color = primary ? new Color32(190, 102, 54, 255) : new Color32(220, 182, 126, 255);
            else
            {
                float variation = ((x * 17 + y * 31) % 7) / 900f;
                color = primary ? Accent : new Color(1f - variation, 0.945f - variation, 0.835f - variation, 1f);
            }
            if (edge >= 0f) color.a = Mathf.Clamp01(edge + 0.5f);
            pixels[y * size + x] = color;
        }
        texture.SetPixels(pixels);
        texture.Apply(false, true);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(14, 14, 14, 14));
        sprite.name = texture.name;
        sprite.hideFlags = HideFlags.HideAndDontSave;
        if (primary) primarySprite = sprite; else paperSprite = sprite;
        return sprite;
    }
}
