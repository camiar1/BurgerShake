using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DraftChoiceVisual : MonoBehaviour
{
    [SerializeField]
    private Image iconImage;

    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private Button button;

    [SerializeField]
    private CanvasGroup canvasGroup;

    private RectTransform rectTransform;
    private IngredientDefinition definition;
    private Action<DraftChoiceVisual> clickCallback;

    private Vector2 hoverCenter;
    private bool feralHover;
    private float feralHoverAmplitude;
    private float feralHoverSpeed;
    private float feralHoverPhase;

    public IngredientDefinition Definition => definition;

    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (canvasGroup == null)
        {
            canvasGroup =
                GetComponent<CanvasGroup>();
        }

        if (button != null)
        {
            button.onClick.AddListener(
                HandleClicked
            );
        }
    }

    private void Update()
    {
        if (!feralHover || rectTransform == null)
        {
            return;
        }

        float time =
            Time.unscaledTime * feralHoverSpeed + feralHoverPhase;

        Vector2 offset =
            new Vector2(
                Mathf.Cos(time) * feralHoverAmplitude,
                Mathf.Sin(time * 1.2f) * feralHoverAmplitude * 0.55f
            );

        rectTransform.anchoredPosition =
            hoverCenter + offset;
    }

    public void Setup(
        IngredientDefinition newDefinition,
        Sprite iconSprite,
        string displayName,
        Action<DraftChoiceVisual> onClicked
    )
    {
        definition = newDefinition;
        clickCallback = onClicked;

        if (iconImage != null)
        {
            iconImage.sprite = iconSprite;
            iconImage.preserveAspect = true;
        }

        if (nameText != null)
        {
            nameText.text = displayName;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    public IEnumerator FlyToSlot(
        Vector2 start,
        Vector2 end,
        float arcHeight,
        float duration,
        bool freezeAtEnd,
        float feralAmplitude,
        float feralSpeed,
        float feralPhase
    )
    {
        feralHover = false;

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = start;
            rectTransform.localScale = Vector3.one;
        }

        float elapsed = 0f;

        Vector2 control =
            (start + end) * 0.5f + Vector2.up * arcHeight;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(elapsed / duration);

            // Fast at the beginning, slower near the end.
            float shaped =
                1f - Mathf.Pow(1f - t, 2.2f);

            Vector2 position =
                EvaluateQuadraticBezier(
                    start,
                    control,
                    end,
                    shaped
                );

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition =
                    position;
            }

            yield return null;
        }

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = end;
        }

        if (freezeAtEnd)
        {
            feralHover = false;
            yield break;
        }

        hoverCenter = end;
        feralHoverAmplitude = feralAmplitude;
        feralHoverSpeed = feralSpeed;
        feralHoverPhase = feralPhase;
        feralHover = true;
    }

    public IEnumerator FadeOutAndDestroy(
        float duration
    )
    {
        feralHover = false;

        if (canvasGroup == null)
        {
            Destroy(gameObject);
            yield break;
        }

        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(elapsed / duration);

            canvasGroup.alpha =
                Mathf.Lerp(startAlpha, 0f, t);

            yield return null;
        }

        canvasGroup.alpha = 0f;

        Destroy(gameObject);
    }

    private void HandleClicked()
    {
        clickCallback?.Invoke(this);
    }

    private Vector2 EvaluateQuadraticBezier(
        Vector2 a,
        Vector2 b,
        Vector2 c,
        float t
    )
    {
        float oneMinusT = 1f - t;

        return
            oneMinusT * oneMinusT * a +
            2f * oneMinusT * t * b +
            t * t * c;
    }
}