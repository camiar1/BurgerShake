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

    private Coroutine jumpRoutine;

    private Vector2 jumpRestPosition;

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

        SetIdle();
    }

    public void SetIdle()
    {
        if (jumpRect != null)
        {
            jumpRect.anchoredPosition =
                jumpRestPosition;
        }

        if (jumpImage != null)
        {
            jumpImage.gameObject
                .SetActive(false);
        }

        if (idleImage != null)
        {
            idleImage.gameObject
                .SetActive(true);
        }
    }

    public void PlayThrowPose()
    {
        if (jumpRoutine != null)
        {
            StopCoroutine(
                jumpRoutine
            );
        }

        jumpRoutine =
            StartCoroutine(
                JumpRoutine()
            );
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

        jumpImage.gameObject
            .SetActive(true);

        jumpRect.anchoredPosition =
            jumpRestPosition;

        Vector2 peakPosition =
            jumpRestPosition +
            Vector2.up *
            jumpHeight;

        // Jump upward.
        yield return MoveJumpImage(
            jumpRestPosition,
            peakPosition,
            jumpUpDuration
        );

        // Tiny pause at the top.
        if (hangDuration > 0f)
        {
            yield return
                new WaitForSecondsRealtime(
                    hangDuration
                );
        }

        // Fall back down behind the ledge.
        yield return MoveJumpImage(
            peakPosition,
            jumpRestPosition,
            jumpDownDuration
        );

        SetIdle();

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
}