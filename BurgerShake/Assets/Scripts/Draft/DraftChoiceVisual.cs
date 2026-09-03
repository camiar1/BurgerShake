using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraftChoiceVisual :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("References")]
    [SerializeField]
    private Image iconImage;

    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private Button button;

    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private RectTransform hoverParticleRoot;

    [Header("Hover Scale")]
    [SerializeField]
    private float hoverScale = 1.25f;

    [SerializeField]
    private float scaleSpeed = 10f;

    [Header("Hover Sway")]
    [SerializeField]
    private float swayAngle = 6f;

    [SerializeField]
    private float swaySpeed = 4f;

    [Header("Hover Particles")]
    [SerializeField]
    private Sprite particleSprite;

    [SerializeField]
    private float particleSpawnInterval = 0.06f;

    [SerializeField]
    private float particleLifetime = 0.45f;

    [SerializeField]
    private float particleSpeed = 80f;

    [SerializeField]
    private float particleSpawnRadius = 28f;

    [SerializeField]
    private float particleSizeMin = 6f;

    [SerializeField]
    private float particleSizeMax = 14f;

    private RectTransform rectTransform;
    private RectTransform iconRect;

    private IngredientDefinition definition;

    private Action<DraftChoiceVisual>
        clickCallback;

    private Vector2 hoverCenter;

    private bool feralHover;

    private float feralHoverAmplitude;
    private float feralHoverSpeed;
    private float feralHoverPhase;

    private bool isHovered;
    private bool hoverEnabled;

    private float particleTimer;

    public IngredientDefinition Definition =>
        definition;

    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();

        if (iconImage != null)
        {
            iconRect =
                iconImage.rectTransform;
        }

        if (button == null)
        {
            button =
                GetComponent<Button>();
        }

        if (canvasGroup == null)
        {
            canvasGroup =
                GetComponent<CanvasGroup>();
        }

        if (button != null)
        {
            button.onClick
                .AddListener(
                    HandleClicked
                );
        }

        ResetHoverVisuals();
    }

    private void Update()
    {
        UpdateFeralHover();
        UpdateHoverVisuals();
        UpdateHoverParticles();
    }

    private void UpdateFeralHover()
    {
        if (
            !feralHover ||
            rectTransform == null
        )
        {
            return;
        }

        float time =
            Time.unscaledTime *
            feralHoverSpeed +
            feralHoverPhase;

        Vector2 offset =
            new Vector2(
                Mathf.Cos(time) *
                feralHoverAmplitude,

                Mathf.Sin(
                    time * 1.2f
                ) *
                feralHoverAmplitude *
                0.55f
            );

        rectTransform.anchoredPosition =
            hoverCenter +
            offset;
    }

    private void UpdateHoverVisuals()
    {
        if (iconRect == null)
        {
            return;
        }

        float targetScale =
            isHovered &&
            hoverEnabled
                ? hoverScale
                : 1f;

        iconRect.localScale =
            Vector3.Lerp(
                iconRect.localScale,
                Vector3.one *
                targetScale,
                scaleSpeed *
                Time.unscaledDeltaTime
            );

        float targetRotation =
            0f;

        if (
            isHovered &&
            hoverEnabled
        )
        {
            targetRotation =
                Mathf.Sin(
                    Time.unscaledTime *
                    swaySpeed
                ) *
                swayAngle;
        }

        Quaternion target =
            Quaternion.Euler(
                0f,
                0f,
                targetRotation
            );

        iconRect.localRotation =
            Quaternion.Lerp(
                iconRect.localRotation,
                target,
                scaleSpeed *
                Time.unscaledDeltaTime
            );
    }

    private void UpdateHoverParticles()
    {
        if (
            !isHovered ||
            !hoverEnabled ||
            hoverParticleRoot == null
        )
        {
            particleTimer =
                0f;

            return;
        }

        particleTimer +=
            Time.unscaledDeltaTime;

        if (
            particleTimer >=
            particleSpawnInterval
        )
        {
            particleTimer =
                0f;

            SpawnHoverParticle();
        }
    }

    private void SpawnHoverParticle()
    {
        GameObject particle =
            new GameObject(
                "HoverParticle",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

        RectTransform particleRect =
            particle.GetComponent<
                RectTransform
            >();

        particleRect.SetParent(
            hoverParticleRoot,
            false
        );

        float angle =
            UnityEngine.Random.Range(
                0f,
                Mathf.PI * 2f
            );

        Vector2 direction =
            new Vector2(
                Mathf.Cos(angle),
                Mathf.Sin(angle)
            );

        particleRect.anchoredPosition =
            direction *
            particleSpawnRadius;

        float size =
            UnityEngine.Random.Range(
                particleSizeMin,
                particleSizeMax
            );

        particleRect.sizeDelta =
            new Vector2(
                size,
                size
            );

        particleRect.localScale =
            Vector3.one;

        Image image =
            particle.GetComponent<Image>();

        image.raycastTarget =
            false;

        image.preserveAspect =
            true;

        if (particleSprite != null)
        {
            image.sprite =
                particleSprite;
        }

        Color color =
            Color.white;

        color.a =
            0.85f;

        image.color =
            color;

        StartCoroutine(
            AnimateParticle(
                particleRect,
                image,
                direction
            )
        );
    }

    private IEnumerator AnimateParticle(
        RectTransform particleRect,
        Image particleImage,
        Vector2 direction
    )
    {
        float elapsed =
            0f;

        Vector2 startPosition =
            particleRect
                .anchoredPosition;

        Vector2 endPosition =
            startPosition +
            direction *
            particleSpeed;

        Vector3 startScale =
            particleRect
                .localScale;

        while (
            elapsed <
            particleLifetime
        )
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    particleLifetime
                );

            particleRect
                .anchoredPosition =
                    Vector2.Lerp(
                        startPosition,
                        endPosition,
                        t
                    );

            particleRect.localScale =
                Vector3.Lerp(
                    startScale,
                    Vector3.zero,
                    t
                );

            Color color =
                particleImage.color;

            color.a =
                Mathf.Lerp(
                    0.85f,
                    0f,
                    t
                );

            particleImage.color =
                color;

            yield return null;
        }

        if (particleRect != null)
        {
            Destroy(
                particleRect
                    .gameObject
            );
        }
    }

    public void Setup(
        IngredientDefinition
            newDefinition,
        Sprite iconSprite,
        string displayName,
        Action<DraftChoiceVisual>
            onClicked
    )
    {
        definition =
            newDefinition;

        clickCallback =
            onClicked;

        if (iconImage != null)
        {
            iconImage.sprite =
                iconSprite;

            iconImage
                .preserveAspect =
                    true;
        }

        if (nameText != null)
        {
            nameText.text =
                displayName;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha =
                1f;
        }

        hoverEnabled =
            false;

        isHovered =
            false;

        ResetHoverVisuals();
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
        feralHover =
            false;

        hoverEnabled =
            false;

        isHovered =
            false;

        ResetHoverVisuals();

        if (rectTransform != null)
        {
            rectTransform
                .anchoredPosition =
                    start;

            rectTransform.localScale =
                Vector3.one;
        }

        float elapsed =
            0f;

        Vector2 control =
            (start + end) *
            0.5f +
            Vector2.up *
            arcHeight;

        while (
            elapsed <
            duration
        )
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    duration
                );

            float shaped =
                1f -
                Mathf.Pow(
                    1f - t,
                    2.2f
                );

            Vector2 position =
                EvaluateQuadraticBezier(
                    start,
                    control,
                    end,
                    shaped
                );

            if (
                rectTransform !=
                null
            )
            {
                rectTransform
                    .anchoredPosition =
                        position;
            }

            yield return null;
        }

        if (rectTransform != null)
        {
            rectTransform
                .anchoredPosition =
                    end;
        }

        hoverEnabled =
            true;

        if (freezeAtEnd)
        {
            feralHover =
                false;

            yield break;
        }

        hoverCenter =
            end;

        feralHoverAmplitude =
            feralAmplitude;

        feralHoverSpeed =
            feralSpeed;

        feralHoverPhase =
            feralPhase;

        feralHover =
            true;
    }

    public IEnumerator
        FadeOutAndDestroy(
            float duration
        )
    {
        feralHover =
            false;

        hoverEnabled =
            false;

        isHovered =
            false;

        ResetHoverVisuals();

        if (canvasGroup == null)
        {
            Destroy(
                gameObject
            );

            yield break;
        }

        float elapsed =
            0f;

        float startAlpha =
            canvasGroup.alpha;

        while (
            elapsed <
            duration
        )
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    duration
                );

            canvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    0f,
                    t
                );

            yield return null;
        }

        canvasGroup.alpha =
            0f;

        Destroy(
            gameObject
        );
    }

    public void OnPointerEnter(
        PointerEventData eventData
    )
    {
        if (!hoverEnabled)
        {
            return;
        }

        isHovered =
            true;
    }

    public void OnPointerExit(
        PointerEventData eventData
    )
    {
        isHovered =
            false;
    }

    private void HandleClicked()
    {
        clickCallback
            ?.Invoke(
                this
            );
    }

    private void ResetHoverVisuals()
    {
        if (iconRect == null)
        {
            return;
        }

        iconRect.localScale =
            Vector3.one;

        iconRect.localRotation =
            Quaternion.identity;
    }

    private Vector2
        EvaluateQuadraticBezier(
            Vector2 a,
            Vector2 b,
            Vector2 c,
            float t
        )
    {
        float oneMinusT =
            1f - t;

        return
            oneMinusT *
            oneMinusT *
            a +

            2f *
            oneMinusT *
            t *
            b +

            t *
            t *
            c;
    }
}