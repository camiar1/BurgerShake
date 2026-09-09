using System.Collections;
using UnityEngine;

public class UISlideAnimator : MonoBehaviour
{
    [Header("Target")]
    [Tooltip(
        "The UI element to move. Leave empty to use this object's RectTransform."
    )]
    [SerializeField]
    private RectTransform target;

    [Tooltip(
        "Optional. Used to prevent interaction while the UI is hidden."
    )]
    [SerializeField]
    private CanvasGroup canvasGroup;

    [Header("Positions")]
    [Tooltip(
        "Offset from the normal position while hidden. " +
        "(0, 400) means hidden above and slides downward when shown."
    )]
    [SerializeField]
    private Vector2 hiddenOffset =
        new Vector2(
            0f,
            400f
        );

    [Header("Animation")]
    [SerializeField]
    [Min(0.01f)]
    private float slideDuration =
        0.35f;

    [SerializeField]
    private AnimationCurve slideCurve =
        AnimationCurve.EaseInOut(
            0f,
            0f,
            1f,
            1f
        );

    [SerializeField]
    private bool useUnscaledTime =
        true;

    [Header("Starting State")]
    [SerializeField]
    private bool startHidden =
        true;

    [SerializeField]
    private bool showOnStart =
        false;

    private Vector2 shownPosition;
    private Vector2 hiddenPosition;

    private Coroutine slideRoutine;

    private bool initialized;
    private bool isShown;

    public bool IsShown =>
        isShown;

    private void Awake()
    {
        Initialize();

        if (startHidden)
        {
            HideInstant();
        }
        else
        {
            ShowInstant();
        }
    }

    private void Start()
    {
        if (showOnStart)
        {
            Show();
        }
    }

    private void OnDisable()
    {
        StopCurrentAnimation();
    }

    private void Initialize()
    {
        if (initialized)
        {
            return;
        }

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
                $"{name}: UISlideAnimator needs a RectTransform."
            );

            return;
        }

        shownPosition =
            target.anchoredPosition;

        hiddenPosition =
            shownPosition +
            hiddenOffset;

        initialized =
            true;
    }

    public void Show()
    {
        Initialize();

        if (target == null)
        {
            return;
        }

        StopCurrentAnimation();

        slideRoutine =
            StartCoroutine(
                ShowRoutine()
            );
    }

    public void Hide()
    {
        Initialize();

        if (target == null)
        {
            return;
        }

        StopCurrentAnimation();

        slideRoutine =
            StartCoroutine(
                HideRoutine()
            );
    }

    public void Toggle()
    {
        if (isShown)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    public void ShowInstant()
    {
        Initialize();

        if (target == null)
        {
            return;
        }

        StopCurrentAnimation();

        target.anchoredPosition =
            shownPosition;

        isShown =
            true;

        SetInteraction(
            true
        );
    }

    public void HideInstant()
    {
        Initialize();

        if (target == null)
        {
            return;
        }

        StopCurrentAnimation();

        target.anchoredPosition =
            hiddenPosition;

        isShown =
            false;

        SetInteraction(
            false
        );
    }

    public IEnumerator ShowRoutine()
    {
        Initialize();

        if (target == null)
        {
            yield break;
        }

        SetInteraction(
            false
        );

        yield return
            SlideTo(
                shownPosition
            );

        isShown =
            true;

        SetInteraction(
            true
        );

        slideRoutine =
            null;
    }

    public IEnumerator HideRoutine()
    {
        Initialize();

        if (target == null)
        {
            yield break;
        }

        SetInteraction(
            false
        );

        yield return
            SlideTo(
                hiddenPosition
            );

        isShown =
            false;

        slideRoutine =
            null;
    }

    private IEnumerator SlideTo(
        Vector2 destination
    )
    {
        Vector2 startPosition =
            target.anchoredPosition;

        float elapsed =
            0f;

        float duration =
            Mathf.Max(
                0.01f,
                slideDuration
            );

        while (elapsed < duration)
        {
            elapsed +=
                GetDeltaTime();

            float normalized =
                Mathf.Clamp01(
                    elapsed /
                    duration
                );

            float curved =
                slideCurve != null
                    ? slideCurve.Evaluate(
                        normalized
                    )
                    : normalized;

            target.anchoredPosition =
                Vector2.LerpUnclamped(
                    startPosition,
                    destination,
                    curved
                );

            yield return null;
        }

        target.anchoredPosition =
            destination;
    }

    private void SetInteraction(
        bool enabled
    )
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.interactable =
            enabled;

        canvasGroup.blocksRaycasts =
            enabled;
    }

    private float GetDeltaTime()
    {
        return useUnscaledTime
            ? Time.unscaledDeltaTime
            : Time.deltaTime;
    }

    private void StopCurrentAnimation()
    {
        if (slideRoutine == null)
        {
            return;
        }

        StopCoroutine(
            slideRoutine
        );

        slideRoutine =
            null;
    }
}