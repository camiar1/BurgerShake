using System.Collections;
using UnityEngine;

public class UIPanelAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Fade")]
    [SerializeField] private bool animateFade = true;

    [Header("Scale")]
    [SerializeField] private bool animateScale = true;
    [SerializeField] private Vector3 hiddenScale = new Vector3(0.9f, 0.9f, 1f);
    [SerializeField] private Vector3 visibleScale = Vector3.one;

    [Header("Slide")]
    [SerializeField] private bool animateSlide;
    [SerializeField] private Vector2 hiddenOffset = new Vector2(0f, -40f);

    [Header("Start State")]
    [SerializeField] private bool hideOnStart;

    private Vector2 visiblePosition;
    private Coroutine animationCoroutine;

    private void Awake()
    {
        FindReferencesIfNeeded();

        if (rectTransform != null)
        {
            visiblePosition = rectTransform.anchoredPosition;
        }

        if (hideOnStart)
        {
            SetHiddenInstant();
        }
    }

    public void Show()
    {
        FindReferencesIfNeeded();
        gameObject.SetActive(true);

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(AnimatePanel(true));
    }

    public void Hide()
    {
        FindReferencesIfNeeded();

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(AnimatePanel(false));
    }

    public void ShowInstant()
    {
        FindReferencesIfNeeded();
        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (rectTransform != null)
        {
            rectTransform.localScale = visibleScale;
            rectTransform.anchoredPosition = visiblePosition;
        }
    }

    public void HideInstant()
    {
        SetHiddenInstant();
    }

    private IEnumerator AnimatePanel(bool showing)
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : (showing ? 0f : 1f);
        float endAlpha = showing ? 1f : 0f;
        Vector3 startScale = rectTransform != null ? rectTransform.localScale : (showing ? hiddenScale : visibleScale);
        Vector3 endScale = showing ? visibleScale : hiddenScale;
        Vector2 startPosition = rectTransform != null ? rectTransform.anchoredPosition : visiblePosition;
        Vector2 endPosition = showing ? visiblePosition : visiblePosition + hiddenOffset;

        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        while (elapsed < animationDuration)
        {
            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            t = EaseOutBack(t);

            if (canvasGroup != null && animateFade)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            }

            if (rectTransform != null && animateScale)
            {
                rectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            }

            if (rectTransform != null && animateSlide)
            {
                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);
            }

            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = endAlpha;
            canvasGroup.interactable = showing;
            canvasGroup.blocksRaycasts = showing;
        }

        if (rectTransform != null)
        {
            rectTransform.localScale = endScale;
            rectTransform.anchoredPosition = endPosition;
        }

        if (!showing)
        {
            gameObject.SetActive(false);
        }

        animationCoroutine = null;
    }

    private void SetHiddenInstant()
    {
        FindReferencesIfNeeded();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (rectTransform != null)
        {
            rectTransform.localScale = hiddenScale;
            rectTransform.anchoredPosition = visiblePosition + hiddenOffset;
        }

        gameObject.SetActive(false);
    }

    private void FindReferencesIfNeeded()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
