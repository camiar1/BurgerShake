using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CatMascotView : MonoBehaviour
{
    [Header("Images")]
    [SerializeField]
    private Image idleImage;

    [SerializeField]
    private Image jumpImage;

    [SerializeField]
    private Image sleepImage;

    [Header("Jump Transform")]
    [SerializeField]
    private RectTransform jumpRect;

    [Header("Jump Motion")]
    [SerializeField]
    private float jumpHeight = 110f;

    [SerializeField]
    private float jumpUpDuration = 0.18f;

    [SerializeField]
    private float hangDuration = 0.08f;

    [SerializeField]
    private float jumpDownDuration = 0.25f;

    [SerializeField]
    private AnimationCurve jumpCurve =
        AnimationCurve.EaseInOut(
            0f,
            0f,
            1f,
            1f
        );

    [Header("Sleep Squish")]
    [SerializeField]
    private Vector2 squishScale =
        new Vector2(
            1.18f,
            0.82f
        );

    [SerializeField]
    private Vector2 reboundScale =
        new Vector2(
            0.94f,
            1.08f
        );

    [SerializeField]
    private float squishInDuration =
        0.10f;

    [SerializeField]
    private float reboundDuration =
        0.08f;

    [SerializeField]
    private float settleDuration =
        0.10f;

    [SerializeField]
    private float sleepDelayAfterSquish =
        0.03f;

    private Coroutine jumpRoutine;
    private Coroutine sleepRoutine;

    private RectTransform idleRect;

    private Vector2 jumpRestPosition;
    private Vector3 idleRestScale;

    public bool IsSleeping
    {
        get;
        private set;
    }

    private void Awake()
    {
        if (
            jumpRect == null &&
            jumpImage != null
        )
        {
            jumpRect =
                jumpImage.rectTransform;
        }

        if (jumpRect != null)
        {
            jumpRestPosition =
                jumpRect.anchoredPosition;
        }

        if (idleImage != null)
        {
            idleRect =
                idleImage.rectTransform;

            idleRestScale =
                idleRect.localScale;
        }

        SetIdle();
    }

    private void OnDisable()
    {
        if (jumpRoutine != null)
        {
            StopCoroutine(
                jumpRoutine
            );

            jumpRoutine =
                null;
        }

        if (sleepRoutine != null)
        {
            StopCoroutine(
                sleepRoutine
            );

            sleepRoutine =
                null;
        }

        SetIdleImmediate();
    }

    public void SetIdle()
    {
        if (sleepRoutine != null)
        {
            StopCoroutine(
                sleepRoutine
            );

            sleepRoutine =
                null;
        }

        SetIdleImmediate();
    }

    private void SetIdleImmediate()
    {
        IsSleeping =
            false;

        ResetJumpPosition();
        ResetIdleScale();

        if (jumpImage != null)
        {
            jumpImage.gameObject
                .SetActive(false);
        }

        if (sleepImage != null)
        {
            sleepImage.gameObject
                .SetActive(false);
        }

        if (idleImage != null)
        {
            idleImage.gameObject
                .SetActive(true);
        }
    }

    public void SetSleeping()
    {
        if (
            IsSleeping ||
            sleepRoutine != null
        )
        {
            return;
        }

        if (jumpRoutine != null)
        {
            StopCoroutine(
                jumpRoutine
            );

            jumpRoutine =
                null;
        }

        sleepRoutine =
            StartCoroutine(
                SleepRoutine()
            );
    }

    public void PlayThrowPose()
    {
        IsSleeping =
            false;

        if (sleepRoutine != null)
        {
            StopCoroutine(
                sleepRoutine
            );

            sleepRoutine =
                null;
        }

        if (jumpRoutine != null)
        {
            StopCoroutine(
                jumpRoutine
            );
        }

        ResetIdleScale();

        if (sleepImage != null)
        {
            sleepImage.gameObject
                .SetActive(false);
        }

        jumpRoutine =
            StartCoroutine(
                JumpRoutine()
            );
    }

    private IEnumerator SleepRoutine()
    {
        ResetJumpPosition();
        ResetIdleScale();

        if (jumpImage != null)
        {
            jumpImage.gameObject
                .SetActive(false);
        }

        if (sleepImage != null)
        {
            sleepImage.gameObject
                .SetActive(false);
        }

        if (idleImage != null)
        {
            idleImage.gameObject
                .SetActive(true);
        }

        if (idleRect != null)
        {
            Vector3 squished =
                new Vector3(
                    idleRestScale.x *
                    squishScale.x,

                    idleRestScale.y *
                    squishScale.y,

                    idleRestScale.z
                );

            Vector3 rebound =
                new Vector3(
                    idleRestScale.x *
                    reboundScale.x,

                    idleRestScale.y *
                    reboundScale.y,

                    idleRestScale.z
                );

            yield return ScaleIdle(
                idleRestScale,
                squished,
                squishInDuration
            );

            yield return ScaleIdle(
                squished,
                rebound,
                reboundDuration
            );

            yield return ScaleIdle(
                rebound,
                idleRestScale,
                settleDuration
            );
        }

        if (
            sleepDelayAfterSquish >
            0f
        )
        {
            yield return
                new WaitForSecondsRealtime(
                    sleepDelayAfterSquish
                );
        }

        if (idleImage != null)
        {
            idleImage.gameObject
                .SetActive(false);
        }

        if (sleepImage != null)
        {
            sleepImage.gameObject
                .SetActive(true);
        }

        IsSleeping =
            true;

        sleepRoutine =
            null;
    }

    private IEnumerator ScaleIdle(
        Vector3 start,
        Vector3 end,
        float duration
    )
    {
        if (idleRect == null)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            idleRect.localScale =
                end;

            yield break;
        }

        float elapsed =
            0f;

        while (elapsed < duration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    duration
                );

            float smooth =
                t *
                t *
                (
                    3f -
                    2f *
                    t
                );

            idleRect.localScale =
                Vector3.LerpUnclamped(
                    start,
                    end,
                    smooth
                );

            yield return null;
        }

        idleRect.localScale =
            end;
    }

    private IEnumerator JumpRoutine()
    {
        if (
            jumpImage == null ||
            jumpRect == null
        )
        {
            yield break;
        }

        if (idleImage != null)
        {
            idleImage.gameObject
                .SetActive(false);
        }

        if (sleepImage != null)
        {
            sleepImage.gameObject
                .SetActive(false);
        }

        jumpImage.gameObject
            .SetActive(true);

        jumpRect.anchoredPosition =
            jumpRestPosition;

        Vector2 peakPosition =
            jumpRestPosition +
            Vector2.up *
            jumpHeight;

        yield return MoveJumpImage(
            jumpRestPosition,
            peakPosition,
            jumpUpDuration
        );

        if (hangDuration > 0f)
        {
            yield return
                new WaitForSecondsRealtime(
                    hangDuration
                );
        }

        yield return MoveJumpImage(
            peakPosition,
            jumpRestPosition,
            jumpDownDuration
        );

        SetIdleImmediate();

        jumpRoutine =
            null;
    }

    private IEnumerator MoveJumpImage(
        Vector2 start,
        Vector2 end,
        float duration
    )
    {
        if (duration <= 0f)
        {
            jumpRect.anchoredPosition =
                end;

            yield break;
        }

        float elapsed =
            0f;

        while (elapsed < duration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float normalized =
                Mathf.Clamp01(
                    elapsed /
                    duration
                );

            float curved =
                jumpCurve != null
                    ? jumpCurve.Evaluate(
                        normalized
                    )
                    : normalized;

            jumpRect.anchoredPosition =
                Vector2.LerpUnclamped(
                    start,
                    end,
                    curved
                );

            yield return null;
        }

        jumpRect.anchoredPosition =
            end;
    }

    private void ResetJumpPosition()
    {
        if (jumpRect != null)
        {
            jumpRect.anchoredPosition =
                jumpRestPosition;
        }
    }

    private void ResetIdleScale()
    {
        if (idleRect != null)
        {
            idleRect.localScale =
                idleRestScale;
        }
    }
}