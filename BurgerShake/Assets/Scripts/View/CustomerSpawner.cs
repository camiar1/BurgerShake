using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Run")]
    [SerializeField] private RunManager runManager;

    [Header("Customer Visual")]
    [SerializeField] private Image customerImage;
    [SerializeField] private RectTransform customerRect;

    [Header("Movement Points")]
    [SerializeField] private RectTransform entryPoint;
    [SerializeField] private RectTransform waitingPoint;
    [SerializeField] private RectTransform exitPoint;

    [Header("Movement")]
    [SerializeField] private float movementDuration = 1.25f;

    [SerializeField]
    private AnimationCurve movementCurve =
        AnimationCurve.EaseInOut(
            0f,
            0f,
            1f,
            1f
        );

    private CustomerDefinition activeCustomer;

    private Coroutine movementRoutine;

    public CustomerDefinition ActiveCustomer =>
        activeCustomer;

    public bool CustomerWaiting
    {
        get;
        private set;
    }

    public bool CustomerMoving
    {
        get;
        private set;
    }

    public event Action<CustomerDefinition>
        CustomerArrived;

    public event Action
        CustomerLeft;

    private void Awake()
    {
        if (runManager == null)
        {
            runManager =
                FindFirstObjectByType<RunManager>();
        }

        if (
            customerRect == null &&
            customerImage != null
        )
        {
            customerRect =
                customerImage.rectTransform;
        }

        if (customerImage != null)
        {
            customerImage.raycastTarget =
                false;
        }
    }

    private void OnEnable()
    {
        if (runManager != null)
        {
            runManager.CustomerIntroStarted +=
                HandleCustomerIntroStarted;
        }
    }

    private void OnDisable()
    {
        if (runManager != null)
        {
            runManager.CustomerIntroStarted -=
                HandleCustomerIntroStarted;
        }

        StopMovement();
    }

    private void Start()
    {
        HideCustomerImmediately();

        if (
            runManager != null &&
            runManager.State ==
                RunState.CustomerIntro &&
            runManager.CurrentCustomer != null
        )
        {
            ShowCustomer(
                runManager.CurrentCustomer
            );
        }
    }

    private void HandleCustomerIntroStarted(
        CustomerDefinition customer,
        int goalScore
    )
    {
        ShowCustomer(
            customer
        );
    }

    public void ShowCustomer(
        CustomerDefinition customer
    )
    {
        if (
            customer == null ||
            customerImage == null ||
            customerRect == null ||
            entryPoint == null ||
            waitingPoint == null
        )
        {
            return;
        }

        StopMovement();

        activeCustomer =
            customer;

        CustomerWaiting =
            false;

        CustomerMoving =
            true;

        ApplyCustomerVisual(
            customer
        );

        Vector2 start =
            GetPointPosition(
                entryPoint,
                customer
            );

        Vector2 destination =
            GetPointPosition(
                waitingPoint,
                customer
            );

        customerRect.anchoredPosition =
            start;

        customerImage.gameObject
            .SetActive(true);

        movementRoutine =
            StartCoroutine(
                EnterRoutine(
                    start,
                    destination
                )
            );
    }

    public void CustomerLeaves()
    {
        if (
            activeCustomer == null ||
            customerRect == null ||
            exitPoint == null
        )
        {
            CustomerLeft?.Invoke();
            return;
        }

        StopMovement();

        CustomerWaiting =
            false;

        CustomerMoving =
            true;

        Vector2 start =
            customerRect.anchoredPosition;

        Vector2 destination =
            GetPointPosition(
                exitPoint,
                activeCustomer
            );

        movementRoutine =
            StartCoroutine(
                ExitRoutine(
                    start,
                    destination
                )
            );
    }

    private IEnumerator EnterRoutine(
        Vector2 start,
        Vector2 destination
    )
    {
        yield return MoveCustomer(
            start,
            destination
        );

        CustomerMoving =
            false;

        CustomerWaiting =
            true;

        movementRoutine =
            null;

        CustomerArrived?.Invoke(
            activeCustomer
        );
    }

    private IEnumerator ExitRoutine(
        Vector2 start,
        Vector2 destination
    )
    {
        yield return MoveCustomer(
            start,
            destination
        );

        if (customerImage != null)
        {
            customerImage.gameObject
                .SetActive(false);
        }

        activeCustomer =
            null;

        CustomerMoving =
            false;

        CustomerWaiting =
            false;

        movementRoutine =
            null;

        CustomerLeft?.Invoke();
    }

    private IEnumerator MoveCustomer(
        Vector2 start,
        Vector2 destination
    )
    {
        float duration =
            Mathf.Max(
                0.01f,
                movementDuration
            );

        float elapsed =
            0f;

        while (elapsed < duration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float normalized =
                Mathf.Clamp01(
                    elapsed / duration
                );

            float curved =
                movementCurve != null
                    ? movementCurve
                        .Evaluate(normalized)
                    : normalized;

            customerRect.anchoredPosition =
                Vector2.LerpUnclamped(
                    start,
                    destination,
                    curved
                );

            yield return null;
        }

        customerRect.anchoredPosition =
            destination;
    }

    private Vector2 GetPointPosition(
        RectTransform point,
        CustomerDefinition customer
    )
    {
        return
            point.anchoredPosition +
            customer.visualOffset;
    }

    private void ApplyCustomerVisual(
        CustomerDefinition customer
    )
    {
        customerImage.sprite =
            customer.portrait;

        customerImage.enabled =
            customer.portrait != null;

        customerImage.preserveAspect =
            true;

        customerImage.raycastTarget =
            false;

        float scale =
            Mathf.Max(
                0.01f,
                customer.visualScale
            );

        customerRect.localScale =
            new Vector3(
                scale,
                scale,
                1f
            );
    }

    private void StopMovement()
    {
        if (movementRoutine != null)
        {
            StopCoroutine(
                movementRoutine
            );

            movementRoutine =
                null;
        }

        CustomerMoving =
            false;
    }

    private void HideCustomerImmediately()
    {
        StopMovement();

        activeCustomer =
            null;

        CustomerWaiting =
            false;

        CustomerMoving =
            false;

        if (customerImage != null)
        {
            customerImage.gameObject
                .SetActive(false);
        }
    }
}