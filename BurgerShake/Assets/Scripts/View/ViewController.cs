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

    [Header("View Targets")]
    [SerializeField] private Transform assemblyTarget;
    [SerializeField] private Transform customerWindowTarget;

    [Header("Sliding")]
    [SerializeField] private float slideDuration = 0.45f;
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Starting View")]
    [SerializeField] private FoodTruckView startingView = FoodTruckView.CustomerWindow;

    private FoodTruckView currentView;
    private bool isSliding;

    public FoodTruckView CurrentView => currentView;
    public bool IsSliding => isSliding;

    private void Start()
    {
        currentView = startingView;
        SnapToView(currentView);
    }

    public void TurnLeft(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TurnLeft();
        }
    }

    public void TurnRight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TurnRight();
        }
    }

    public void TurnLeft()
    {
        if (isSliding || currentView == FoodTruckView.Assembly)
        {
            return;
        }

        BeginSlide(FoodTruckView.Assembly);
    }

    public void TurnRight()
    {
        if (isSliding || currentView == FoodTruckView.CustomerWindow)
        {
            return;
        }

        BeginSlide(FoodTruckView.CustomerWindow);
    }

    public void GoToAssembly()
    {
        if (!isSliding && currentView != FoodTruckView.Assembly)
        {
            BeginSlide(FoodTruckView.Assembly);
        }
    }

    public void GoToCustomerWindow()
    {
        if (!isSliding && currentView != FoodTruckView.CustomerWindow)
        {
            BeginSlide(FoodTruckView.CustomerWindow);
        }
    }

    private void BeginSlide(FoodTruckView targetView)
    {
        StartCoroutine(SlideToView(targetView));
    }

    private IEnumerator SlideToView(FoodTruckView targetView)
    {
        Transform target = GetTarget(targetView);

        if (target == null)
        {
            Debug.LogError("View target is missing for " + targetView + ".");
            yield break;
        }

        isSliding = true;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / slideDuration);
            float curvedTime = slideCurve.Evaluate(normalizedTime);

            transform.position = Vector3.LerpUnclamped(startPosition, targetPosition, curvedTime);
            yield return null;
        }

        transform.position = targetPosition;
        currentView = targetView;
        isSliding = false;
    }

    private void SnapToView(FoodTruckView view)
    {
        Transform target = GetTarget(view);

        if (target == null)
        {
            Debug.LogError("View target is missing for " + view + ".");
            return;
        }

        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }

    private Transform GetTarget(FoodTruckView view)
    {
        return view == FoodTruckView.Assembly ? assemblyTarget : customerWindowTarget;
    }
}
