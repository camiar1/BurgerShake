using System.Collections;
using UnityEngine;

public class CustomerTravelController : MonoBehaviour
{
    [Header("Customer")]
    [SerializeField] private RectTransform customer;

    [Header("Path")]
    [SerializeField] private RectTransform startPoint;
    [SerializeField] private RectTransform waitPoint;
    [SerializeField] private RectTransform endPoint;

    [Header("Movement")]
    [SerializeField] private float enterDuration = 1.5f;
    [SerializeField] private float exitDuration = 1.5f;

    [SerializeField]
    private AnimationCurve movementCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public bool IsMoving { get; private set; }

    private Coroutine movementRoutine;

    private void Awake()
    {
        if (customer == null)
            customer = GetComponent<RectTransform>();

        ResetToStart();
    }
    private void Start()
    {
        Enter();
    }

    public void Enter()
    {
        MoveCustomer(startPoint, waitPoint, enterDuration);
    }

    public void Leave()
    {
        MoveCustomer(waitPoint, endPoint, exitDuration);
    }

    public void ResetToStart()
    {
        if (movementRoutine != null)
        {
            StopCoroutine(movementRoutine);
            movementRoutine = null;
        }

        IsMoving = false;

        if (customer != null && startPoint != null)
            customer.position = startPoint.position;
    }

    private void MoveCustomer(
        RectTransform from,
        RectTransform to,
        float duration)
    {
        if (from == null || to == null || customer == null)
            return;

        if (movementRoutine != null)
            StopCoroutine(movementRoutine);

        movementRoutine = StartCoroutine(
            MoveRoutine(from.position, to.position, duration)
        );
    }

    private IEnumerator MoveRoutine(
        Vector3 start,
        Vector3 end,
        float duration)
    {
        IsMoving = true;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float normalizedTime =
                Mathf.Clamp01(elapsed / duration);

            float curvedTime =
                movementCurve.Evaluate(normalizedTime);

            customer.position =
                Vector3.LerpUnclamped(
                    start,
                    end,
                    curvedTime
                );

            yield return null;
        }

        customer.position = end;

        IsMoving = false;
        movementRoutine = null;
    }
}