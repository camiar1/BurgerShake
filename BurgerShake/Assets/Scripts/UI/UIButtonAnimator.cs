using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonAnimator : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [SerializeField] private RectTransform rectTransform;

    [Header("Scale Settings")]
    [SerializeField] private Vector3 normalScale = Vector3.one;
    [SerializeField] private Vector3 hoverScale = new Vector3(1.06f, 1.06f, 1f);
    [SerializeField] private Vector3 pressedScale = new Vector3(0.94f, 0.94f, 1f);

    [Header("Timing")]
    [SerializeField] private float animationDuration = 0.08f;
    [SerializeField] private bool useUnscaledTime = true;

    private Coroutine scaleCoroutine;
    private bool pointerIsOver;

    private void Awake()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (rectTransform != null)
        {
            rectTransform.localScale = normalScale;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerIsOver = true;
        AnimateToScale(hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerIsOver = false;
        AnimateToScale(normalScale);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        AnimateToScale(pressedScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        AnimateToScale(pointerIsOver ? hoverScale : normalScale);
    }

    private void AnimateToScale(Vector3 targetScale)
    {
        if (rectTransform == null)
        {
            return;
        }

        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }

        scaleCoroutine = StartCoroutine(ScaleRoutine(targetScale));
    }

    private IEnumerator ScaleRoutine(Vector3 targetScale)
    {
        Vector3 startScale = rectTransform.localScale;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += deltaTime;

            float t = Mathf.Clamp01(elapsed / animationDuration);
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);

            yield return null;
        }

        rectTransform.localScale = targetScale;
        scaleCoroutine = null;
    }
}
