using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonAnimator :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    ISelectHandler,
    IDeselectHandler
{
    [Header("References")]
    [Tooltip(
        "The RectTransform that will animate. " +
        "Leave empty to animate this GameObject."
    )]
    [SerializeField]
    private RectTransform visualTarget;

    [SerializeField]
    private Button button;

    [Header("Hover")]
    [SerializeField]
    [Min(1f)]
    private float hoverScale = 1.06f;

    [SerializeField]
    [Min(0.01f)]
    private float hoverDuration = 0.1f;

    [Header("Press")]
    [SerializeField]
    [Range(0.5f, 1f)]
    private float pressedScale = 0.92f;

    [SerializeField]
    [Min(0.01f)]
    private float pressDuration = 0.06f;

    [Header("Release Pop")]
    [SerializeField]
    [Min(1f)]
    private float releasePopScale = 1.1f;

    [SerializeField]
    [Min(0.01f)]
    private float releasePopDuration = 0.07f;

    [SerializeField]
    [Min(0.01f)]
    private float releaseSettleDuration = 0.08f;

    [Header("Options")]
    [SerializeField]
    private bool animateKeyboardSelection = true;

    [SerializeField]
    private bool useUnscaledTime = true;

    private Vector3 baseScale;

    private Coroutine animationRoutine;

    private bool pointerIsOver;
    private bool pointerIsDown;
    private bool isSelected;

    private void Awake()
    {
        if (visualTarget == null)
        {
            visualTarget =
                GetComponent<RectTransform>();
        }

        if (button == null)
        {
            button =
                GetComponent<Button>();
        }

        if (visualTarget != null)
        {
            baseScale =
                visualTarget.localScale;
        }
    }

    private void OnEnable()
    {
        pointerIsOver =
            false;

        pointerIsDown =
            false;

        isSelected =
            false;

        if (visualTarget != null)
        {
            visualTarget.localScale =
                baseScale;
        }
    }

    private void OnDisable()
    {
        if (animationRoutine != null)
        {
            StopCoroutine(
                animationRoutine
            );

            animationRoutine =
                null;
        }

        if (visualTarget != null)
        {
            visualTarget.localScale =
                baseScale;
        }
    }

    public void OnPointerEnter(
        PointerEventData eventData
    )
    {
        pointerIsOver =
            true;

        if (!CanAnimate())
        {
            return;
        }

        if (!pointerIsDown)
        {
            AnimateTo(
                hoverScale,
                hoverDuration
            );
        }
    }

    public void OnPointerExit(
        PointerEventData eventData
    )
    {
        pointerIsOver =
            false;

        if (!CanAnimate())
        {
            return;
        }

        if (!pointerIsDown)
        {
            AnimateTo(
                GetRestingScale(),
                hoverDuration
            );
        }
    }

    public void OnPointerDown(
        PointerEventData eventData
    )
    {
        if (!CanAnimate())
        {
            return;
        }

        pointerIsDown =
            true;

        AnimateTo(
            pressedScale,
            pressDuration
        );
    }

    public void OnPointerUp(
        PointerEventData eventData
    )
    {
        if (!CanAnimate())
        {
            return;
        }

        pointerIsDown =
            false;

        PlayReleasePop();
    }

    public void OnSelect(
        BaseEventData eventData
    )
    {
        if (!animateKeyboardSelection)
        {
            return;
        }

        isSelected =
            true;

        if (!CanAnimate())
        {
            return;
        }

        if (!pointerIsDown)
        {
            AnimateTo(
                hoverScale,
                hoverDuration
            );
        }
    }

    public void OnDeselect(
        BaseEventData eventData
    )
    {
        if (!animateKeyboardSelection)
        {
            return;
        }

        isSelected =
            false;

        if (!CanAnimate())
        {
            return;
        }

        if (
            !pointerIsDown &&
            !pointerIsOver
        )
        {
            AnimateTo(
                1f,
                hoverDuration
            );
        }
    }

    private bool CanAnimate()
    {
        if (visualTarget == null)
        {
            return false;
        }

        if (
            button != null &&
            !button.interactable
        )
        {
            return false;
        }

        return true;
    }

    private float GetRestingScale()
    {
        if (
            pointerIsOver ||
            (
                animateKeyboardSelection &&
                isSelected
            )
        )
        {
            return hoverScale;
        }

        return 1f;
    }

    private void AnimateTo(
        float scaleMultiplier,
        float duration
    )
    {
        StopCurrentAnimation();

        animationRoutine =
            StartCoroutine(
                ScaleRoutine(
                    scaleMultiplier,
                    duration
                )
            );
    }

    private void PlayReleasePop()
    {
        StopCurrentAnimation();

        animationRoutine =
            StartCoroutine(
                ReleasePopRoutine()
            );
    }

    private IEnumerator ScaleRoutine(
        float targetMultiplier,
        float duration
    )
    {
        Vector3 startScale =
            visualTarget.localScale;

        Vector3 targetScale =
            baseScale *
            targetMultiplier;

        float elapsed =
            0f;

        float safeDuration =
            Mathf.Max(
                0.01f,
                duration
            );

        while (elapsed < safeDuration)
        {
            elapsed +=
                GetDeltaTime();

            float t =
                Mathf.Clamp01(
                    elapsed /
                    safeDuration
                );

            float eased =
                EaseOutCubic(
                    t
                );

            visualTarget.localScale =
                Vector3.LerpUnclamped(
                    startScale,
                    targetScale,
                    eased
                );

            yield return null;
        }

        visualTarget.localScale =
            targetScale;

        animationRoutine =
            null;
    }

    private IEnumerator ReleasePopRoutine()
    {
        Vector3 startScale =
            visualTarget.localScale;

        Vector3 popScale =
            baseScale *
            releasePopScale;

        float elapsed =
            0f;

        float popDuration =
            Mathf.Max(
                0.01f,
                releasePopDuration
            );

        while (elapsed < popDuration)
        {
            elapsed +=
                GetDeltaTime();

            float t =
                Mathf.Clamp01(
                    elapsed /
                    popDuration
                );

            float eased =
                EaseOutCubic(
                    t
                );

            visualTarget.localScale =
                Vector3.LerpUnclamped(
                    startScale,
                    popScale,
                    eased
                );

            yield return null;
        }

        float restingMultiplier =
            GetRestingScale();

        Vector3 restingScale =
            baseScale *
            restingMultiplier;

        Vector3 settleStart =
            visualTarget.localScale;

        elapsed =
            0f;

        float settleDuration =
            Mathf.Max(
                0.01f,
                releaseSettleDuration
            );

        while (
            elapsed <
            settleDuration
        )
        {
            elapsed +=
                GetDeltaTime();

            float t =
                Mathf.Clamp01(
                    elapsed /
                    settleDuration
                );

            float eased =
                EaseOutBack(
                    t
                );

            visualTarget.localScale =
                Vector3.LerpUnclamped(
                    settleStart,
                    restingScale,
                    eased
                );

            yield return null;
        }

        visualTarget.localScale =
            restingScale;

        animationRoutine =
            null;
    }

    private void StopCurrentAnimation()
    {
        if (animationRoutine == null)
        {
            return;
        }

        StopCoroutine(
            animationRoutine
        );

        animationRoutine =
            null;
    }

    private float GetDeltaTime()
    {
        return useUnscaledTime
            ? Time.unscaledDeltaTime
            : Time.deltaTime;
    }

    private float EaseOutCubic(
        float t
    )
    {
        return
            1f -
            Mathf.Pow(
                1f - t,
                3f
            );
    }

    private float EaseOutBack(
        float t
    )
    {
        const float overshoot =
            1.70158f;

        const float amount =
            overshoot + 1f;

        float value =
            t - 1f;

        return
            1f +
            amount *
            value *
            value *
            value +
            overshoot *
            value *
            value;
    }
}