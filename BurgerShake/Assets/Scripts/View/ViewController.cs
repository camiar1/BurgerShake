using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ViewController : MonoBehaviour
{
    public enum FoodTruckView
    {
        Assembly,
        CustomerWindow
    }

    [Header("Station Layout")]
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform stationStrip;

    [SerializeField] private RectTransform assemblyView;
    [SerializeField] private RectTransform customerWindowView;

    [Header("View Interaction")]
    [SerializeField] private CanvasGroup assemblyCanvasGroup;
    [SerializeField] private CanvasGroup customerWindowCanvasGroup;

    [Header("Assembly Physics")]
    [SerializeField] private Transform assemblyPhysicsRoot;

    [Header("Sliding")]
    [SerializeField] private float slideDuration = 0.45f;

    [SerializeField]
    private AnimationCurve slideCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Starting View")]
    [SerializeField]
    private FoodTruckView startingView =
        FoodTruckView.CustomerWindow;

    private FoodTruckView currentView;

    private bool isSliding;

    private float viewWidth;
    private float viewHeight;

    public FoodTruckView CurrentView => currentView;

    public bool IsSliding => isSliding;

    public bool IsReady { get; private set; }

    public event Action<FoodTruckView> ViewChanged;

    private IEnumerator Start()
    {
        IsReady = false;

        currentView = startingView;

        yield return null;

        Canvas.ForceUpdateCanvases();

        RefreshLayout();

        SnapToView(currentView);

        UpdateViewInteraction();

        SetAssemblyPhysicsSimulation(
            currentView == FoodTruckView.Assembly
        );

        IsReady = true;
    }

    private void OnRectTransformDimensionsChange()
    {
        if (
            !isActiveAndEnabled ||
            viewport == null ||
            !IsReady
        )
        {
            return;
        }

        RefreshLayout();

        if (!isSliding)
        {
            SnapToView(currentView);
        }
    }

    // Legacy methods.
    // Nothing in the current game should call these.
    // They remain here so the old ViewKeyboardInput
    // script does not cause compile errors.
    public void TurnLeft(
        InputAction.CallbackContext context
    )
    {
        if (context.performed)
        {
            TurnLeft();
        }
    }

    public void TurnRight(
        InputAction.CallbackContext context
    )
    {
        if (context.performed)
        {
            TurnRight();
        }
    }

    public void TurnLeft()
    {
        if (
            isSliding ||
            currentView == FoodTruckView.Assembly
        )
        {
            return;
        }

        BeginSlide(FoodTruckView.Assembly);
    }

    public void TurnRight()
    {
        if (
            isSliding ||
            currentView ==
            FoodTruckView.CustomerWindow
        )
        {
            return;
        }

        BeginSlide(
            FoodTruckView.CustomerWindow
        );
    }

    public void GoToAssembly()
    {
        if (
            isSliding ||
            currentView == FoodTruckView.Assembly
        )
        {
            return;
        }

        BeginSlide(
            FoodTruckView.Assembly
        );
    }

    public void GoToCustomerWindow()
    {
        if (
            isSliding ||
            currentView ==
            FoodTruckView.CustomerWindow
        )
        {
            return;
        }

        BeginSlide(
            FoodTruckView.CustomerWindow
        );
    }

    private void BeginSlide(
        FoodTruckView targetView
    )
    {
        StartCoroutine(
            SlideToView(targetView)
        );
    }

    private IEnumerator SlideToView(
        FoodTruckView targetView
    )
    {
        isSliding = true;

        DisableAllInteraction();

        SetAssemblyPhysicsSimulation(false);

        Vector2 start =
            stationStrip.anchoredPosition;

        Vector2 target =
            GetStripPosition(targetView);

        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float normalized =
                Mathf.Clamp01(
                    elapsed / slideDuration
                );

            float curved =
                slideCurve.Evaluate(normalized);

            stationStrip.anchoredPosition =
                Vector2.LerpUnclamped(
                    start,
                    target,
                    curved
                );

            yield return null;
        }

        stationStrip.anchoredPosition =
            target;

        currentView = targetView;

        isSliding = false;

        SetAssemblyPhysicsSimulation(
            currentView ==
            FoodTruckView.Assembly
        );

        UpdateViewInteraction();

        ViewChanged?.Invoke(currentView);
    }

    private void RefreshLayout()
    {
        if (
            viewport == null ||
            stationStrip == null ||
            assemblyView == null ||
            customerWindowView == null
        )
        {
            return;
        }

        viewWidth = viewport.rect.width;
        viewHeight = viewport.rect.height;

        stationStrip.anchorMin =
            new Vector2(0.5f, 0.5f);

        stationStrip.anchorMax =
            new Vector2(0.5f, 0.5f);

        stationStrip.pivot =
            new Vector2(0.5f, 0.5f);

        stationStrip.sizeDelta =
            new Vector2(
                viewWidth * 2f,
                viewHeight
            );

        ConfigureView(
            assemblyView,
            -viewWidth * 0.5f
        );

        ConfigureView(
            customerWindowView,
            viewWidth * 0.5f
        );
    }

    private void ConfigureView(
        RectTransform view,
        float xPosition
    )
    {
        view.anchorMin =
            new Vector2(0.5f, 0.5f);

        view.anchorMax =
            new Vector2(0.5f, 0.5f);

        view.pivot =
            new Vector2(0.5f, 0.5f);

        view.sizeDelta =
            new Vector2(
                viewWidth,
                viewHeight
            );

        view.anchoredPosition =
            new Vector2(
                xPosition,
                0f
            );
    }

    private Vector2 GetStripPosition(
        FoodTruckView view
    )
    {
        if (
            view ==
            FoodTruckView.Assembly
        )
        {
            return new Vector2(
                viewWidth * 0.5f,
                0f
            );
        }

        return new Vector2(
            -viewWidth * 0.5f,
            0f
        );
    }

    private void SnapToView(
        FoodTruckView view
    )
    {
        if (stationStrip == null)
        {
            return;
        }

        stationStrip.anchoredPosition =
            GetStripPosition(view);
    }

    private void DisableAllInteraction()
    {
        SetCanvasInteraction(
            assemblyCanvasGroup,
            false
        );

        SetCanvasInteraction(
            customerWindowCanvasGroup,
            false
        );
    }

    private void UpdateViewInteraction()
    {
        SetCanvasInteraction(
            assemblyCanvasGroup,
            currentView ==
            FoodTruckView.Assembly
        );

        SetCanvasInteraction(
            customerWindowCanvasGroup,
            currentView ==
            FoodTruckView.CustomerWindow
        );
    }

    private void SetCanvasInteraction(
        CanvasGroup group,
        bool active
    )
    {
        if (group == null)
        {
            return;
        }

        group.alpha = 1f;
        group.interactable = active;
        group.blocksRaycasts = active;
    }

    private void SetAssemblyPhysicsSimulation(
        bool simulated
    )
    {
        if (assemblyPhysicsRoot == null)
        {
            return;
        }

        Rigidbody2D[] bodies =
            assemblyPhysicsRoot
                .GetComponentsInChildren<Rigidbody2D>(
                    true
                );

        foreach (Rigidbody2D body in bodies)
        {
            body.simulated = simulated;
        }
    }
}