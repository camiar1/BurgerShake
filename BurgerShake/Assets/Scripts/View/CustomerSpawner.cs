using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Available Customers")]
    [SerializeField]
    private List<CustomerDefinition> customers =
        new List<CustomerDefinition>();

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
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private CustomerDefinition activeCustomer;

    private bool customerWaiting;
    private bool customerMoving;

    public CustomerDefinition ActiveCustomer => activeCustomer;
    public bool CustomerWaiting => customerWaiting;

    private void Start()
    {
        HideCustomerImmediately();

        // Temporary for testing.
        SpawnCustomer();
    }

    public void SpawnCustomer()
    {
        if (customerWaiting || customerMoving)
            return;

        if (customers.Count == 0)
        {
            Debug.LogError("No customers assigned.");
            return;
        }

        CustomerDefinition customer =
            customers[Random.Range(0, customers.Count)];

        SpawnCustomer(customer);
    }

    public void SpawnCustomer(CustomerDefinition customer)
    {
        if (customer == null || customerWaiting || customerMoving)
            return;

        StartCoroutine(EnterCustomerRoutine(customer));
    }

    private IEnumerator EnterCustomerRoutine(
        CustomerDefinition customer)
    {
        customerMoving = true;
        activeCustomer = customer;

        ApplyCustomerVisual(customer);

        Vector2 entryPosition =
            entryPoint.anchoredPosition +
            customer.visualOffset;

        Vector2 waitingPosition =
            waitingPoint.anchoredPosition +
            customer.visualOffset;

        customerRect.anchoredPosition = entryPosition;

        customerImage.gameObject.SetActive(true);

        yield return MoveCustomer(
            entryPosition,
            waitingPosition
        );

        customerWaiting = true;
        customerMoving = false;
    }

    public void CustomerLeaves()
    {
        if (activeCustomer == null || customerMoving)
            return;

        customerWaiting = false;

        StartCoroutine(ExitCustomerRoutine());
    }

    private IEnumerator ExitCustomerRoutine()
    {
        customerMoving = true;

        Vector2 startPosition =
            customerRect.anchoredPosition;

        Vector2 exitPosition =
            exitPoint.anchoredPosition +
            activeCustomer.visualOffset;

        yield return MoveCustomer(
            startPosition,
            exitPosition
        );

        customerImage.gameObject.SetActive(false);

        activeCustomer = null;
        customerMoving = false;
    }

    private IEnumerator MoveCustomer(
        Vector2 startPosition,
        Vector2 targetPosition)
    {
        float duration =
            Mathf.Max(0.01f, movementDuration);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float normalizedTime =
                Mathf.Clamp01(
                    elapsedTime / duration
                );

            float curvedTime =
                movementCurve != null
                    ? movementCurve.Evaluate(normalizedTime)
                    : normalizedTime;

            customerRect.anchoredPosition =
                Vector2.LerpUnclamped(
                    startPosition,
                    targetPosition,
                    curvedTime
                );

            yield return null;
        }

        customerRect.anchoredPosition =
            targetPosition;
    }

    private void ApplyCustomerVisual(
        CustomerDefinition customer)
    {
        customerImage.sprite = customer.portrait;
        customerImage.preserveAspect = true;

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

    private void HideCustomerImmediately()
    {
        customerWaiting = false;
        customerMoving = false;
        activeCustomer = null;

        if (customerImage != null)
            customerImage.gameObject.SetActive(false);
    }
}