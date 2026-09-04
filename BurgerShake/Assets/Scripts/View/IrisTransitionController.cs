using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IrisTransitionController :
    MonoBehaviour
{
    [Header("Overlay")]
    [SerializeField]
    private Image irisOverlay;

    [SerializeField]
    private Shader irisShader;

    [SerializeField]
    private Color irisColor =
        Color.black;

    [Header("Timing")]
    [SerializeField]
    private float closeDuration =
        0.4f;

    [SerializeField]
    private float blackHoldDuration =
        0.08f;

    [SerializeField]
    private float openDuration =
        0.4f;

    [Header("Animation")]
    [SerializeField]
    private AnimationCurve transitionCurve =
        AnimationCurve.EaseInOut(
            0f,
            0f,
            1f,
            1f
        );

    [Header("Edge")]
    [SerializeField]
    [Range(0f, 0.05f)]
    private float edgeSoftness =
        0.003f;

    private Material irisMaterial;

    private float currentAspect =
        16f / 9f;

    private static readonly int
        IrisColorProperty =
            Shader.PropertyToID(
                "_IrisColor"
            );

    private static readonly int
        IrisCenterProperty =
            Shader.PropertyToID(
                "_IrisCenter"
            );

    private static readonly int
        IrisRadiusProperty =
            Shader.PropertyToID(
                "_IrisRadius"
            );

    private static readonly int
        EdgeSoftnessProperty =
            Shader.PropertyToID(
                "_EdgeSoftness"
            );

    private static readonly int
        AspectProperty =
            Shader.PropertyToID(
                "_Aspect"
            );

    public bool IsTransitioning
    {
        get;
        private set;
    }

    private void Awake()
    {
        EnsureMaterial();

        SetOverlayActive(
            false
        );
    }

    private void OnDestroy()
    {
        if (irisMaterial != null)
        {
            Destroy(
                irisMaterial
            );

            irisMaterial =
                null;
        }
    }

    public IEnumerator PlayTransition(
        Action whenFullyClosed,
        Vector2 closeCenter,
        Vector2 openCenter
    )
    {
        if (IsTransitioning)
        {
            yield break;
        }

        if (!EnsureMaterial())
        {
            whenFullyClosed
                ?.Invoke();

            yield break;
        }

        IsTransitioning =
            true;

        closeCenter =
            ClampCenter(
                closeCenter
            );

        openCenter =
            ClampCenter(
                openCenter
            );

        UpdateMaterialSettings();

        SetOverlayActive(
            true
        );

        float closeMaxRadius =
            GetMaximumRadius(
                closeCenter
            );

        SetIrisCenter(
            closeCenter
        );

        SetIrisRadius(
            closeMaxRadius
        );

        yield return
            AnimateRadius(
                closeMaxRadius,
                0f,
                closeDuration
            );

        SetIrisRadius(
            0f
        );

        if (blackHoldDuration > 0f)
        {
            yield return
                new WaitForSecondsRealtime(
                    blackHoldDuration
                );
        }

        whenFullyClosed
            ?.Invoke();

        // Gives Unity one frame to render
        // the newly activated view while
        // everything is still black.
        yield return null;

        UpdateMaterialSettings();

        SetIrisCenter(
            openCenter
        );

        float openMaxRadius =
            GetMaximumRadius(
                openCenter
            );

        SetIrisRadius(
            0f
        );

        yield return
            AnimateRadius(
                0f,
                openMaxRadius,
                openDuration
            );

        SetIrisRadius(
            openMaxRadius
        );

        SetOverlayActive(
            false
        );

        IsTransitioning =
            false;
    }

    private IEnumerator AnimateRadius(
        float from,
        float to,
        float duration
    )
    {
        if (duration <= 0f)
        {
            SetIrisRadius(
                to
            );

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
                transitionCurve != null
                    ? transitionCurve
                        .Evaluate(
                            normalized
                        )
                    : normalized;

            float radius =
                Mathf.LerpUnclamped(
                    from,
                    to,
                    curved
                );

            SetIrisRadius(
                radius
            );

            yield return null;
        }

        SetIrisRadius(
            to
        );
    }

    private bool EnsureMaterial()
    {
        if (
            irisOverlay == null
        )
        {
            Debug.LogError(
                "IrisTransitionController " +
                "has no Iris Overlay assigned."
            );

            return false;
        }

        if (
            irisMaterial != null
        )
        {
            return true;
        }

        if (irisShader == null)
        {
            Debug.LogError(
                "IrisTransitionController " +
                "has no Iris Shader assigned."
            );

            return false;
        }

        irisMaterial =
            new Material(
                irisShader
            );

        irisMaterial.name =
            "Runtime Iris Transition";

        irisOverlay.material =
            irisMaterial;

        return true;
    }

    private void UpdateMaterialSettings()
    {
        if (irisMaterial == null)
        {
            return;
        }

        currentAspect =
            Screen.height > 0
                ? (float)Screen.width /
                    Screen.height
                : 16f / 9f;

        irisMaterial.SetColor(
            IrisColorProperty,
            irisColor
        );

        irisMaterial.SetFloat(
            EdgeSoftnessProperty,
            edgeSoftness
        );

        irisMaterial.SetFloat(
            AspectProperty,
            currentAspect
        );
    }

    private void SetIrisCenter(
        Vector2 center
    )
    {
        if (irisMaterial == null)
        {
            return;
        }

        irisMaterial.SetVector(
            IrisCenterProperty,
            new Vector4(
                center.x,
                center.y,
                0f,
                0f
            )
        );
    }

    private void SetIrisRadius(
        float radius
    )
    {
        if (irisMaterial == null)
        {
            return;
        }

        irisMaterial.SetFloat(
            IrisRadiusProperty,
            radius
        );
    }

    private float GetMaximumRadius(
        Vector2 center
    )
    {
        float horizontalDistance =
            Mathf.Max(
                center.x,
                1f - center.x
            ) *
            currentAspect;

        float verticalDistance =
            Mathf.Max(
                center.y,
                1f - center.y
            );

        return
            Mathf.Sqrt(
                horizontalDistance *
                horizontalDistance +
                verticalDistance *
                verticalDistance
            ) +
            edgeSoftness *
            2f;
    }

    private Vector2 ClampCenter(
        Vector2 center
    )
    {
        return new Vector2(
            Mathf.Clamp01(
                center.x
            ),
            Mathf.Clamp01(
                center.y
            )
        );
    }

    private void SetOverlayActive(
        bool active
    )
    {
        if (irisOverlay == null)
        {
            return;
        }

        irisOverlay.gameObject
            .SetActive(
                active
            );
    }
}