using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Temporary presentation shared by the scene's existing shop and ingredient cards.
public static class ShopCardLayout
{
    public static Sprite OpenCrateSprite => Resources.Load<Sprite>("ShopIcons/OpenCrate");

    public static RectTransform Root(string name, Transform parent)
    {
        RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    public static void Place(RectTransform rect, float left, float bottom, float right, float top)
    {
        rect.anchorMin = new Vector2(left, bottom);
        rect.anchorMax = new Vector2(right, top);
        rect.pivot = new Vector2(.5f, .5f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    public static TMP_Text Label(string name, Transform parent, float size)
    {
        TMP_Text label = Root(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
        label.fontSize = size;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(1, .94f, .80f);
        label.raycastTarget = false;
        return label;
    }

    private static void Text(TMP_Text text, Transform parent, float l, float b, float r, float t, float size)
    {
        if (text == null) return;
        text.transform.SetParent(parent, false);
        Place(text.rectTransform, l, b, r, t);
        BurgerShakeGeneratedUITheme.Text(text, size);
        text.enableAutoSizing = true;
        text.fontSizeMin = 12;
        text.fontSizeMax = size;
        text.alignment = TextAlignmentOptions.Center;
        text.color = BurgerShakeGeneratedUITheme.Ink;
        text.raycastTarget = false;
    }

    private static void Frame(Transform card)
    {
        Image image = card.GetComponent<Image>();
        if (image == null) image = card.gameObject.AddComponent<Image>();
        BurgerShakeGeneratedUITheme.Surface(image);
        Button button = card.GetComponent<Button>();
        if (button != null) BurgerShakeGeneratedUITheme.Button(button, null, false);
    }

    private static void Icon(Image icon, Transform card, float l, float b, float r, float t)
    {
        if (icon == null) return;
        icon.transform.SetParent(card, false);
        Place(icon.rectTransform, l, b, r, t);
        icon.color = Color.white;
    }

    public static void StyleOffer(Transform card, Image icon, TMP_Text type, TMP_Text name,
        TMP_Text description, TMP_Text cost, TMP_Text action)
    {
        Frame(card);
        Icon(icon, card, .04f, .26f, .34f, .87f);
        Text(type, card, .36f, .82f, .96f, .95f, 16);
        Text(name, card, .35f, .63f, .96f, .83f, 25);
        BurgerShakeGeneratedUITheme.Text(name, 25, true);
        Text(description, card, .35f, .23f, .96f, .63f, 18);
        Text(cost, card, .04f, .04f, .43f, .22f, 20);
        Text(action, card, .50f, .04f, .95f, .22f, 20);
    }

    public static void StyleIngredient(Transform card, Image icon, TMP_Text name, TMP_Text copies, TMP_Text description)
    {
        Frame(card);
        Icon(icon, card, .20f, .55f, .80f, .82f);
        Text(name, card, .05f, .82f, .95f, .97f, 26);
        BurgerShakeGeneratedUITheme.Text(name, 26, true);
        Text(copies, card, .05f, .44f, .95f, .55f, 19);
        Text(description, card, .08f, .13f, .92f, .43f, 18);
        TMP_Text action = card.Find("Choose label")?.GetComponent<TMP_Text>();
        if (action == null) action = Label("Choose label", card, 20);
        Text(action, card, .05f, .02f, .95f, .12f, 20);
        action.text = "CHOOSE";
    }
}
