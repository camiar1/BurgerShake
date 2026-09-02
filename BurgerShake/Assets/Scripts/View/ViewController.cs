using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ViewController :
    MonoBehaviour
{
    public enum FoodTruckView
    {
        Assembly,
        CustomerWindow
    }

    [Header("Views")]
    [SerializeField]
    private RectTransform assemblyView;

    [SerializeField]
    private RectTransform customerWindowView;

    [Header("View Interaction")]
    [SerializeField]
    private CanvasGroup assemblyCanvasGroup;

    [SerializeField]
    private CanvasGroup customerWindowCanvasGroup;

    [Header("Assembly Physics")]
    [SerializeField]
    private Transform assemblyPhysicsRoot;

    [Header("Iris Transition")]
    [SerializeField]
    private IrisTransitionController
        irisTransition;

    [Tooltip(
        "Normalized screen position where " +
        "the Assembly iris opens/closes. " +
        "(0,0) is bottom-left and " +
        "(1,1) is top-right."
    )]
    [SerializeField]
    private Vector2 assemblyIrisCenter =
        new Vector2(
            0.5f,
            0.5f
        );

    [Tooltip(
        "Normalized screen position where " +
        "the Customer Window iris " +
        "opens/closes."
    )]
    [SerializeField]
    private Vector2 customerWindowIrisCenter =
        new Vector2(
            0.5f,
            0.5f
        );

    [Header("Starting View")]
    [SerializeField]
    private FoodTruckView startingView =
        FoodTruckView.CustomerWindow;

    private FoodTruckView currentView;

    private bool isTransitioning;

    public FoodTruckView CurrentView =>
        currentView;

    // Kept for compatibility with scripts
    // that used the old sliding system.
    public bool IsSliding =>
        isTransitioning;

    public bool IsTransitioning =>
        isTransitioning;

    public bool IsReady
    {
        get;
        private set;
    }

    public event Action<FoodTruckView>
        ViewChanged;

    private void Awake()
    {
        IsReady =
            false;

        currentView =
            startingView;

        ConfigureViews();

        ApplyViewVisibility(
            currentView
        );

        UpdateViewInteraction();

        SetAssemblyPhysicsSimulation(
            currentView ==
            FoodTruckView.Assembly
        );
    }

    private IEnumerator Start()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();

        ConfigureViews();

        ApplyViewVisibility(
            currentView
        );

        UpdateViewInteraction();

        SetAssemblyPhysicsSimulation(
            currentView ==
            FoodTruckView.Assembly
        );

        IsReady =
            true;
    }

    // Legacy Input System callbacks.
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
            isTransitioning ||
            currentView ==
                FoodTruckView.Assembly
        )
        {
            return;
        }

        BeginTransition(
            FoodTruckView.Assembly
        );
    }

    public void TurnRight()
    {
        if (
            isTransitioning ||
            currentView ==
                FoodTruckView.CustomerWindow
        )
        {
            return;
        }

        BeginTransition(
            FoodTruckView.CustomerWindow
        );
    }

    public void GoToAssembly()
    {
        if (
            isTransitioning ||
            currentView ==
                FoodTruckView.Assembly
        )
        {
            return;
        }

        BeginTransition(
            FoodTruckView.Assembly
        );
    }

    public void GoToCustomerWindow()
    {
        if (
            isTransitioning ||
            currentView ==
                FoodTruckView.CustomerWindow
        )
        {
            return;
        }

        BeginTransition(
            FoodTruckView.CustomerWindow
        );
    }

    private void BeginTransition(
        FoodTruckView targetView
    )
    {
        StartCoroutine(
            TransitionToView(
                targetView
            )
        );
    }

    private IEnumerator TransitionToView(
        FoodTruckView targetView
    )
    {
        isTransitioning =
            true;

        IsReady =
            false;

        // Disable clicking, but KEEP the
        // current view visible.
        DisableAllInteraction();

        // Freeze assembly ingredients while
        // the transition is happening.
        SetAssemblyPhysicsSimulation(
            false
        );

        FoodTruckView oldView =
            currentView;

        Vector2 closeCenter =
            GetIrisCenter(
                oldView
            );

        Vector2 openCenter =
            GetIrisCenter(
                targetView
            );

        if (irisTransition != null)
        {
            yield return
                irisTransition
                    .PlayTransition(
                        () =>
                        {
                            // This callback only happens
                            // once the iris is completely
                            // closed and the screen is black.
                            SwitchVisibleView(
                                targetView
                            );
                        },
                        closeCenter,
                        openCenter
                    );
        }
        else
        {
            Debug.LogWarning(
                "ViewController has no " +
                "IrisTransitionController. " +
                "Switching views instantly."
            );

            SwitchVisibleView(
                targetView
            );

            yield return null;
        }

        isTransitioning =
            false;

        SetAssemblyPhysicsSimulation(
            currentView ==
            FoodTruckView.Assembly
        );

        UpdateViewInteraction();

        IsReady =
            true;

        ViewChanged?.Invoke(
            currentView
        );
    }

    private void SwitchVisibleView(
        FoodTruckView targetView
    )
    {
        currentView =
            targetView;

        ApplyViewVisibility(
            currentView
        );
    }

    private void ConfigureViews()
    {
        ConfigureView(
            assemblyView
        );

        ConfigureView(
            customerWindowView
        );
    }

    private void ConfigureView(
        RectTransform view
    )
    {
        if (view == null)
        {
            return;
        }

        view.anchorMin =
            Vector2.zero;

        view.anchorMax =
            Vector2.one;

        view.pivot =
            new Vector2(
                0.5f,
                0.5f
            );

        view.offsetMin =
            Vector2.zero;

        view.offsetMax =
            Vector2.zero;

        view.localScale =
            Vector3.one;

        view.localRotation =
            Quaternion.identity;
    }

    private void ApplyViewVisibility(
        FoodTruckView visibleView
    )
    {
        bool showAssembly =
            visibleView ==
            FoodTruckView.Assembly;

        bool showCustomerWindow =
            visibleView ==
            FoodTruckView.CustomerWindow;

        SetViewVisibility(
            assemblyView,
            assemblyCanvasGroup,
            showAssembly
        );

        SetViewVisibility(
            customerWindowView,
            customerWindowCanvasGroup,
            showCustomerWindow
        );
    }

    private void SetViewVisibility(
        RectTransform view,
        CanvasGroup group,
        bool visible
    )
    {
        if (view != null)
        {
            view.gameObject
                .SetActive(
                    visible
                );
        }

        if (group != null)
        {
            group.alpha =
                visible
                    ? 1f
                    : 0f;

            group.interactable =
                false;

            group.blocksRaycasts =
                false;
        }
    }

    private void DisableAllInteraction()
    {
        // IMPORTANT:
        // Do not change alpha here.
        // The current view must remain visible
        // while the iris closes over it.

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

        // Only control input here.
        // Visibility is handled separately
        // by ApplyViewVisibility().
        group.interactable =
            active;

        group.blocksRaycasts =
            active;
    }

    private Vector2 GetIrisCenter(
        FoodTruckView view
    )
    {
        if (
            view ==
            FoodTruckView.Assembly
        )
        {
            return
                assemblyIrisCenter;
        }

        return
            customerWindowIrisCenter;
    }

    private void SetAssemblyPhysicsSimulation(
        bool simulated
    )
    {
        if (
            assemblyPhysicsRoot == null
        )
        {
            return;
        }

        Rigidbody2D[] bodies =
            assemblyPhysicsRoot
                .GetComponentsInChildren<
                    Rigidbody2D
                >(
                    true
                );

        foreach (
            Rigidbody2D body
            in bodies
        )
        {
            body.simulated =
                simulated;
        }
    }
}