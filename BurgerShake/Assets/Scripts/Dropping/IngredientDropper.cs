using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class IngredientDropper : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField]
    private Camera gameplayCamera;

    [SerializeField]
    private ViewController viewController;

    [SerializeField]
    private RunManager runManager;

    [SerializeField]
    private GameplayModifiers gameplayModifiers;

    [Header("Drop Area")]
    [SerializeField]
    private Collider2D dropRegion;

    [Tooltip(
        "When enabled, the preview only follows the mouse horizontally."
    )]
    [SerializeField]
    private bool lockPreviewToDropY = true;

    [SerializeField]
    private float dropY = 4f;

    [Header("Top Opening Limits")]
    [Tooltip(
        "Inside-left edge of the blender's top opening."
    )]
    [SerializeField]
    private Transform topLeftDropLimit;

    [Tooltip(
        "Inside-right edge of the blender's top opening."
    )]
    [SerializeField]
    private Transform topRightDropLimit;

    [Tooltip(
        "Small extra gap between the ingredient collider and blender rim."
    )]
    [SerializeField]
    [Min(0f)]
    private float edgePadding = 0f;

    [Header("Ingredients")]
    [SerializeField]
    private Transform ingredientContainer;

    [SerializeField]
    private int droppedIngredientSortingOrder = 12;

    [Header("Preview")]
    [Range(0.1f, 1f)]
    [SerializeField]
    private float previewAlpha = 0.72f;

    [SerializeField]
    private int previewSortingOrderOffset = 1;

    [Header("Preview Animation")]
    [Tooltip(
        "How long the preview takes to pop into view."
    )]
    [SerializeField]
    [Min(0.01f)]
    private float previewAppearDuration = 0.12f;

    [Tooltip(
        "Starting scale of the preview when it appears."
    )]
    [SerializeField]
    [Range(0.5f, 1f)]
    private float previewStartScale = 0.9f;

    [Tooltip(
        "Temporary scale multiplier when rotating."
    )]
    [SerializeField]
    [Range(1f, 1.15f)]
    private float rotationPulseScale = 1.025f;

    [Tooltip(
        "Duration of the small rotation pulse."
    )]
    [SerializeField]
    [Min(0.01f)]
    private float rotationPulseDuration = 0.08f;

    [Header("Rotation")]
    [Tooltip(
        "Degrees rotated for each mouse-wheel step."
    )]
    [SerializeField]
    [Min(1f)]
    private float rotationStepDegrees = 15f;

    private IngredientDefinition selectedIngredient;
    private IngredientDraftManager draftManager;

    private GameObject previewObject;
    private SpriteRenderer previewRenderer;

    private Vector3 previewBaseScale;

    private float previewRotation;

    private float previewShownTime;
    private float rotationPulseTime = -100f;

    private int dropsThisChallenge;

    public bool HasIngredient =>
        selectedIngredient != null;

    public bool PreviewVisible =>
        previewObject != null &&
        previewObject.activeSelf;

    public int DropsThisChallenge =>
        dropsThisChallenge;

    public void Initialize(
        IngredientDraftManager manager
    )
    {
        draftManager =
            manager;
    }

    private void Awake()
    {
        if (gameplayCamera == null)
        {
            gameplayCamera =
                Camera.main;
        }

        if (viewController == null)
        {
            viewController =
                FindFirstObjectByType<
                    ViewController
                >();
        }

        if (runManager == null)
        {
            runManager =
                FindFirstObjectByType<
                    RunManager
                >();
        }
    }

    private void OnDisable()
    {
        HidePreview();
    }

    public void ResetChallenge()
    {
        selectedIngredient =
            null;

        dropsThisChallenge =
            0;

        previewRotation =
            0f;

        DestroyPreview();

        draftManager
            ?.ClearCurrentChoices();
    }

    public void SetIngredient(
        IngredientDefinition ingredient
    )
    {
        if (
            ingredient == null ||
            HasReachedDropLimit()
        )
        {
            return;
        }

        selectedIngredient =
            ingredient;

        previewRotation =
            0f;

        rotationPulseTime =
            -100f;

        CreatePreview();
    }

    private void Update()
    {
        if (
            selectedIngredient == null ||
            gameplayCamera == null
        )
        {
            HidePreview();
            return;
        }

        if (!CanAcceptGameplayInput())
        {
            HidePreview();
            return;
        }

        if (
            viewController != null &&
            (
                viewController.IsSliding ||
                viewController.CurrentView !=
                    ViewController
                        .FoodTruckView
                        .Assembly
            )
        )
        {
            HidePreview();
            return;
        }

        Mouse mouse =
            Mouse.current;

        if (mouse == null)
        {
            HidePreview();
            return;
        }

        if (
            EventSystem.current != null &&
            EventSystem.current
                .IsPointerOverGameObject()
        )
        {
            HidePreview();
            return;
        }

        Vector3 mouseWorld =
            gameplayCamera
                .ScreenToWorldPoint(
                    mouse.position
                        .ReadValue()
                );

        mouseWorld.z =
            0f;

        if (
            !IsInsideDropRegionVertically(
                mouseWorld
            )
        )
        {
            HidePreview();
            return;
        }

        ShowPreview();

        // Rotation is updated first so the
        // horizontal limits immediately use
        // the new orientation.
        UpdatePreviewRotation(
            mouse
        );

        UpdatePreviewPosition(
            mouseWorld
        );

        UpdatePreviewPolish();

        if (
            mouse.leftButton
                .wasPressedThisFrame
        )
        {
            DropIngredient();
        }
    }

    private bool IsInsideDropRegionVertically(
        Vector2 worldPosition
    )
    {
        if (dropRegion == null)
        {
            return true;
        }

        Bounds bounds =
            dropRegion.bounds;

        return
            worldPosition.y >=
                bounds.min.y &&
            worldPosition.y <=
                bounds.max.y;
    }

    private void UpdatePreviewPosition(
        Vector3 mouseWorld
    )
    {
        if (previewObject == null)
        {
            return;
        }

        Vector3 position =
            mouseWorld;

        if (lockPreviewToDropY)
        {
            position.y =
                dropY;
        }

        ClampPreviewToTopOpening(
            ref position
        );

        position.z =
            transform.position.z;

        previewObject
            .transform
            .position =
                position;
    }

    private void ClampPreviewToTopOpening(
        ref Vector3 position
    )
    {
        if (
            topLeftDropLimit == null ||
            topRightDropLimit == null
        )
        {
            return;
        }

        float leftX =
            Mathf.Min(
                topLeftDropLimit.position.x,
                topRightDropLimit.position.x
            );

        float rightX =
            Mathf.Max(
                topLeftDropLimit.position.x,
                topRightDropLimit.position.x
            );

        CalculatePreviewHorizontalExtents(
            out float leftExtent,
            out float rightExtent
        );

        float minimumX =
            leftX +
            leftExtent +
            edgePadding;

        float maximumX =
            rightX -
            rightExtent -
            edgePadding;

        if (minimumX > maximumX)
        {
            position.x =
                (
                    leftX +
                    rightX
                ) *
                0.5f;

            return;
        }

        position.x =
            Mathf.Clamp(
                position.x,
                minimumX,
                maximumX
            );
    }

    private void CalculatePreviewHorizontalExtents(
        out float leftExtent,
        out float rightExtent
    )
    {
        leftExtent =
            0f;

        rightExtent =
            0f;

        if (
            previewObject == null ||
            selectedIngredient == null ||
            selectedIngredient.prefab == null
        )
        {
            return;
        }

        PolygonCollider2D polygonCollider =
            selectedIngredient
                .prefab
                .GetComponentInChildren<
                    PolygonCollider2D
                >();

        if (
            polygonCollider != null &&
            TryCalculatePolygonColliderExtents(
                polygonCollider,
                out leftExtent,
                out rightExtent
            )
        )
        {
            return;
        }

        CalculateSpriteExtents(
            out leftExtent,
            out rightExtent
        );
    }

    private bool TryCalculatePolygonColliderExtents(
        PolygonCollider2D polygonCollider,
        out float leftExtent,
        out float rightExtent
    )
    {
        leftExtent =
            0f;

        rightExtent =
            0f;

        if (
            polygonCollider == null ||
            polygonCollider.pathCount <= 0 ||
            previewObject == null ||
            selectedIngredient == null ||
            selectedIngredient.prefab == null
        )
        {
            return false;
        }

        Matrix4x4 previewMatrix =
            GetPreviewBaseLocalToWorldMatrix();

        Transform prefabRoot =
            selectedIngredient
                .prefab
                .transform;

        Matrix4x4 colliderToPrefab =
            prefabRoot.worldToLocalMatrix *
            polygonCollider
                .transform
                .localToWorldMatrix;

        Vector3 originWorld =
            previewMatrix
                .MultiplyPoint3x4(
                    Vector3.zero
                );

        float minimumWorldX =
            float.PositiveInfinity;

        float maximumWorldX =
            float.NegativeInfinity;

        bool foundPoint =
            false;

        for (
            int pathIndex = 0;
            pathIndex <
                polygonCollider.pathCount;
            pathIndex++
        )
        {
            Vector2[] points =
                polygonCollider
                    .GetPath(
                        pathIndex
                    );

            for (
                int pointIndex = 0;
                pointIndex <
                    points.Length;
                pointIndex++
            )
            {
                Vector2 colliderPoint =
                    points[
                        pointIndex
                    ] +
                    polygonCollider.offset;

                Vector3 prefabLocalPoint =
                    colliderToPrefab
                        .MultiplyPoint3x4(
                            colliderPoint
                        );

                Vector3 worldPoint =
                    previewMatrix
                        .MultiplyPoint3x4(
                            prefabLocalPoint
                        );

                minimumWorldX =
                    Mathf.Min(
                        minimumWorldX,
                        worldPoint.x
                    );

                maximumWorldX =
                    Mathf.Max(
                        maximumWorldX,
                        worldPoint.x
                    );

                foundPoint =
                    true;
            }
        }

        if (!foundPoint)
        {
            return false;
        }

        leftExtent =
            Mathf.Max(
                0f,
                originWorld.x -
                minimumWorldX
            );

        rightExtent =
            Mathf.Max(
                0f,
                maximumWorldX -
                originWorld.x
            );

        return true;
    }

    private void CalculateSpriteExtents(
        out float leftExtent,
        out float rightExtent
    )
    {
        leftExtent =
            0f;

        rightExtent =
            0f;

        if (
            previewRenderer == null ||
            previewRenderer.sprite == null ||
            previewObject == null
        )
        {
            return;
        }

        Bounds spriteBounds =
            previewRenderer
                .sprite
                .bounds;

        float minX =
            spriteBounds.min.x;

        float maxX =
            spriteBounds.max.x;

        float minY =
            spriteBounds.min.y;

        float maxY =
            spriteBounds.max.y;

        if (previewRenderer.flipX)
        {
            float oldMinX =
                minX;

            minX =
                -maxX;

            maxX =
                -oldMinX;
        }

        if (previewRenderer.flipY)
        {
            float oldMinY =
                minY;

            minY =
                -maxY;

            maxY =
                -oldMinY;
        }

        Vector3[] corners =
            new Vector3[4]
            {
                new Vector3(
                    minX,
                    minY,
                    0f
                ),
                new Vector3(
                    minX,
                    maxY,
                    0f
                ),
                new Vector3(
                    maxX,
                    minY,
                    0f
                ),
                new Vector3(
                    maxX,
                    maxY,
                    0f
                )
            };

        Matrix4x4 previewMatrix =
            GetPreviewBaseLocalToWorldMatrix();

        Vector3 originWorld =
            previewMatrix
                .MultiplyPoint3x4(
                    Vector3.zero
                );

        float minimumWorldX =
            float.PositiveInfinity;

        float maximumWorldX =
            float.NegativeInfinity;

        for (
            int i = 0;
            i < corners.Length;
            i++
        )
        {
            Vector3 worldPoint =
                previewMatrix
                    .MultiplyPoint3x4(
                        corners[i]
                    );

            minimumWorldX =
                Mathf.Min(
                    minimumWorldX,
                    worldPoint.x
                );

            maximumWorldX =
                Mathf.Max(
                    maximumWorldX,
                    worldPoint.x
                );
        }

        leftExtent =
            Mathf.Max(
                0f,
                originWorld.x -
                minimumWorldX
            );

        rightExtent =
            Mathf.Max(
                0f,
                maximumWorldX -
                originWorld.x
            );
    }

    private Matrix4x4
        GetPreviewBaseLocalToWorldMatrix()
    {
        Transform previewTransform =
            previewObject.transform;

        Matrix4x4 localMatrix =
            Matrix4x4.TRS(
                previewTransform
                    .localPosition,
                previewTransform
                    .localRotation,
                previewBaseScale
            );

        if (
            previewTransform.parent ==
            null
        )
        {
            return localMatrix;
        }

        return
            previewTransform
                .parent
                .localToWorldMatrix *
            localMatrix;
    }

    private void UpdatePreviewRotation(
        Mouse mouse
    )
    {
        if (
            previewObject == null ||
            !previewObject.activeSelf
        )
        {
            return;
        }

        float scroll =
            mouse.scroll
                .ReadValue()
                .y;

        if (
            Mathf.Abs(scroll) >
            0.01f
        )
        {
            float direction =
                Mathf.Sign(
                    scroll
                );

            previewRotation +=
                direction *
                rotationStepDegrees;

            previewRotation =
                Mathf.Repeat(
                    previewRotation,
                    360f
                );

            rotationPulseTime =
                Time.unscaledTime;
        }

        previewObject
            .transform
            .rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    previewRotation
                );
    }

    private void UpdatePreviewPolish()
    {
        if (
            previewObject == null ||
            previewRenderer == null ||
            !previewObject.activeSelf
        )
        {
            return;
        }

        float appearTime =
            Mathf.Max(
                0.01f,
                previewAppearDuration
            );

        float appearProgress =
            Mathf.Clamp01(
                (
                    Time.unscaledTime -
                    previewShownTime
                ) /
                appearTime
            );

        float easedAppear =
            Mathf.SmoothStep(
                0f,
                1f,
                appearProgress
            );

        float appearScale =
            Mathf.Lerp(
                previewStartScale,
                1f,
                easedAppear
            );

        float pulseScale =
            1f;

        float timeSinceRotation =
            Time.unscaledTime -
            rotationPulseTime;

        if (
            timeSinceRotation >= 0f &&
            timeSinceRotation <
                rotationPulseDuration
        )
        {
            float pulseProgress =
                Mathf.Clamp01(
                    timeSinceRotation /
                    Mathf.Max(
                        0.01f,
                        rotationPulseDuration
                    )
                );

            float pulse =
                Mathf.Sin(
                    pulseProgress *
                    Mathf.PI
                );

            pulseScale +=
                (
                    rotationPulseScale -
                    1f
                ) *
                pulse;
        }

        previewObject
            .transform
            .localScale =
                previewBaseScale *
                appearScale *
                pulseScale;

        Color color =
            Color.white;

        color.a =
            previewAlpha *
            easedAppear;

        previewRenderer.color =
            color;
    }

    private void CreatePreview()
    {
        DestroyPreview();

        if (
            selectedIngredient == null ||
            selectedIngredient.prefab == null
        )
        {
            return;
        }

        SpriteRenderer prefabRenderer =
            selectedIngredient
                .prefab
                .GetComponentInChildren<
                    SpriteRenderer
                >();

        Sprite sprite =
            selectedIngredient.sprite;

        if (
            sprite == null &&
            prefabRenderer != null
        )
        {
            sprite =
                prefabRenderer.sprite;
        }

        if (sprite == null)
        {
            return;
        }

        previewObject =
            new GameObject(
                "IngredientPreview"
            );

        if (transform.parent != null)
        {
            previewObject
                .transform
                .SetParent(
                    transform.parent,
                    true
                );
        }

        previewRenderer =
            previewObject
                .AddComponent<
                    SpriteRenderer
                >();

        previewRenderer.sprite =
            sprite;

        if (prefabRenderer != null)
        {
            previewRenderer
                .sortingLayerID =
                    prefabRenderer
                        .sortingLayerID;

            previewRenderer.flipX =
                prefabRenderer.flipX;

            previewRenderer.flipY =
                prefabRenderer.flipY;
        }

        previewRenderer.sortingOrder =
            droppedIngredientSortingOrder +
            previewSortingOrderOffset;

        float scaleMultiplier =
            gameplayModifiers != null
                ? gameplayModifiers
                    .IngredientScale
                : 1f;

        previewBaseScale =
            selectedIngredient
                .prefab
                .transform
                .localScale *
            scaleMultiplier;

        previewObject
            .transform
            .localScale =
                previewBaseScale *
                previewStartScale;

        previewObject
            .transform
            .rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    previewRotation
                );

        Color color =
            Color.white;

        color.a =
            0f;

        previewRenderer.color =
            color;

        previewObject
            .SetActive(
                false
            );
    }

    private void ShowPreview()
    {
        if (previewObject == null)
        {
            return;
        }

        if (
            !previewObject
                .activeSelf
        )
        {
            previewShownTime =
                Time.unscaledTime;

            previewObject
                .SetActive(
                    true
                );
        }
    }

    private void HidePreview()
    {
        if (previewObject != null)
        {
            previewObject
                .SetActive(
                    false
                );
        }
    }

    private void DestroyPreview()
    {
        if (previewObject == null)
        {
            return;
        }

        Destroy(
            previewObject
        );

        previewObject =
            null;

        previewRenderer =
            null;
    }

    private void DropIngredient()
    {
        if (
            selectedIngredient == null ||
            selectedIngredient.prefab == null ||
            HasReachedDropLimit()
        )
        {
            return;
        }

        if (
            previewObject == null ||
            !previewObject.activeSelf
        )
        {
            return;
        }

        if (ingredientContainer == null)
        {
            Debug.LogError(
                "IngredientDropper has no IngredientContainer."
            );

            return;
        }

        Quaternion rotation =
            Quaternion.Euler(
                0f,
                0f,
                previewRotation
            );

        GameObject spawned =
            Instantiate(
                selectedIngredient.prefab,
                previewObject
                    .transform
                    .position,
                rotation,
                ingredientContainer
            );

        float scaleMultiplier =
            gameplayModifiers != null
                ? gameplayModifiers
                    .IngredientScale
                : 1f;

        spawned
            .transform
            .localScale *=
                scaleMultiplier;

        SpriteRenderer[] renderers =
            spawned
                .GetComponentsInChildren<
                    SpriteRenderer
                >(
                    true
                );

        foreach (
            SpriteRenderer renderer
            in renderers
        )
        {
            renderer.sortingOrder =
                droppedIngredientSortingOrder;
        }

        Ingredient ingredient =
            spawned
                .GetComponent<
                    Ingredient
                >();

        if (ingredient != null)
        {
            ingredient.Initialize(
                selectedIngredient
            );
        }

        dropsThisChallenge++;

        selectedIngredient =
            null;

        previewRotation =
            0f;

        DestroyPreview();

        draftManager
            ?.IngredientWasDropped();
    }

    private bool CanAcceptGameplayInput()
    {
        return
            runManager == null ||
            runManager.State ==
                RunState.Assembly;
    }

    private bool HasReachedDropLimit()
    {
        return
            gameplayModifiers != null &&
            gameplayModifiers.DropLimit >
                0 &&
            dropsThisChallenge >=
                gameplayModifiers
                    .DropLimit;
    }
}