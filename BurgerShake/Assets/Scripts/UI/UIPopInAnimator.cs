using System.Collections;
using UnityEngine;

public class UIPopInAnimator : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private RectTransform target;

    [SerializeField]
    private CanvasGroup canvasGroup;

    [Header("Pop")]
    [SerializeField]
    [Min(0.01f)]
    private float popDuration = 0.22f;

    [SerializeField]
    [Min(1f)]
    private float overshootScale = 1.08f;

    [Header("Hide")]
    [SerializeField]
    [Min(0.01f)]
    private float hideDuration = 0.12f;

    [Header("Settings")]
    [SerializeField]
    private bool hideOnStart = true;

    [SerializeField]
    private bool useUnscaledTime = true;

    private Vector3 shownScale;

    private Coroutine animationRoutine;

    public bool IsVisible
    {
        get;
        private set;
    }

    private void Awake()
    {
        if (target == null)
        {
            target =
                GetComponent<RectTransform>();
        }

        if (
            canvasGroup == null &&
            target != null
        )
        {
            canvasGroup =
                target.GetComponent<
                    CanvasGroup
                >();
        }

        if (target == null)
        {
            Debug.LogError(
                $"{name}: UIPopInAnimator needs a RectTransform."
            );

            return;
        }

        shownScale =
            target.localScale;

        if (hideOnStart)
        {
            HideInstant();
        }
        else
        {
            ShowInstant();
        }
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    public void Show()
    {
        if (target == null)
        {
            return;
        }

        StopAnimation();

        animationRoutine =
            StartCoroutine(
                ShowRoutine()
            );
    }

    public void Hide()
    {
        if (target == null)
        {
            return;
        }

        StopAnimation();

        animationRoutine =
            StartCoroutine(
                HideRoutine()
            );
    }

    public void ShowInstant()
    {
        if (target == null)
        {
            return;
        }

        StopAnimation();

        target.localScale =
            shownScale;

        SetVisibleState(
            true
        );

        IsVisible =
            true;
    }

    public void HideInstant()
    {
        if (target == null)
        {
            return;
        }

        StopAnimation();

        target.localScale =
            Vector3.zero;

        SetVisibleState(
            false
        );

        IsVisible =
            false;
    }

    private IEnumerator ShowRoutine()
    {
        SetVisibleState(
            true
        );

        target.localScale =
            Vector3.zero;

        float totalDuration =
            Mathf.Max(
                0.01f,
                popDuration
            );

        float growDuration =
            totalDuration *
            0.72f;

        float settleDuration =
            totalDuration *
            0.28f;

        Vector3 overshoot =
            shownScale *
            overshootScale;

        float elapsed =
            0f;

        while (
            elapsed <
            growDuration
        )
        {
            elapsed +=
                GetDeltaTime();

            float t =
                Mathf.Clamp01(
                    elapsed /
                    growDuration
                );

            float eased =
                EaseOutCubic(
                    t
                );

            target.localScale =
                Vector3.LerpUnclamped(
                    Vector3.zero,
                    overshoot,
                    eased
                );

            yield return null;
        }

        target.localScale =
            overshoot;

        elapsed =
            0f;

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
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            target.localScale =
                Vector3.LerpUnclamped(
                    overshoot,
                    shownScale,
                    eased
                );

            yield return null;
        }

        target.localScale =
            shownScale;

        IsVisible =
            true;

        animationRoutine =
            null;
    }

    private IEnumerator HideRoutine()
    {
        Vector3 startScale =
            target.localScale;

        float duration =
            Mathf.Max(
                0.01f,
                hideDuration
            );

        float elapsed =
            0f;

        while (
            elapsed <
            duration
        )
        {
            elapsed +=
                GetDeltaTime();

            float t =
                Mathf.Clamp01(
                    elapsed /
                    duration
                );

            float eased =
                t * t * t;

            target.localScale =
                Vector3.LerpUnclamped(
                    startScale,
                    Vector3.zero,
                    eased
                );

            yield return null;
        }

        target.localScale =
            Vector3.zero;

        SetVisibleState(
            false
        );

        IsVisible =
            false;

        animationRoutine =
            null;
    }

    private void SetVisibleState(
        bool visible
    )
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha =
            visible
                ? 1f
                : 0f;

        canvasGroup.interactable =
            visible;

        canvasGroup.blocksRaycasts =
            visible;
    }

    private void StopAnimation()
    {
        if (
            animationRoutine ==
            null
        )
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
}