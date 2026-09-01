using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class IngredientDropper : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private ViewController viewController;
    [SerializeField] private RunManager runManager;
    [SerializeField] private GameplayModifiers gameplayModifiers;

    [Header("Drop Area")]
    [SerializeField] private Collider2D dropRegion;

    [Tooltip(
        "When enabled, the preview only follows the mouse horizontally."
    )]
    [SerializeField] private bool lockPreviewToDropY = true;

    [SerializeField] private float dropY = 4f;

    [Header("Ingredients")]
    [SerializeField] private Transform ingredientContainer;

    [Header("Preview")]
    [Range(0.1f, 1f)]
    [SerializeField] private float previewAlpha = 0.65f;

    [SerializeField] private int previewSortingOrderOffset = 1;

    [Header("Rotation")]
    [Tooltip(
        "Degrees per second while holding the right mouse button."
    )]
    [SerializeField] private float rotationSpeed = 140f;

    private IngredientDefinition selectedIngredient;
    private IngredientDraftManager draftManager;

    private GameObject previewObject;
    private SpriteRenderer previewRenderer;

    private float previewRotation;

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
        draftManager = manager;
    }

    private void Awake()
    {
        if (gameplayCamera == null)
        {
            gameplayCamera = Camera.main;
        }

        if (viewController == null)
        {
            viewController =
                FindFirstObjectByType<ViewController>();
        }

        if (runManager == null)
        {
            runManager =
                FindFirstObjectByType<RunManager>();
        }
    }

    private void OnDisable()
    {
        HidePreview();
    }

    public void ResetChallenge()
    {
        selectedIngredient = null;

        dropsThisChallenge = 0;

        previewRotation = 0f;

        DestroyPreview();

        draftManager?.RefreshChoices();
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

        previewRotation = 0f;

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
            gameplayCamera.ScreenToWorldPoint(
                mouse.position.ReadValue()
            );

        mouseWorld.z = 0f;

        bool insideDropRegion =
            IsInsideDropRegion(
                mouseWorld
            );

        if (!insideDropRegion)
        {
            HidePreview();
            return;
        }

        ShowPreview();

        UpdatePreviewPosition(
            mouseWorld
        );

        UpdatePreviewRotation(
            mouse
        );

        if (
            mouse.leftButton
                .wasPressedThisFrame
        )
        {
            DropIngredient();
        }
    }

    private bool IsInsideDropRegion(
        Vector2 worldPosition
    )
    {
        if (dropRegion == null)
        {
            return true;
        }

        return dropRegion.OverlapPoint(
            worldPosition
        );
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

        position.z =
            transform.position.z;

        previewObject.transform.position =
            position;
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

        if (mouse.rightButton.isPressed)
        {
            previewRotation -=
                rotationSpeed *
                Time.unscaledDeltaTime;
        }

        previewObject.transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                previewRotation
            );
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
            selectedIngredient.prefab
                .GetComponentInChildren<SpriteRenderer>();

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
            previewObject.transform.SetParent(
                transform.parent,
                true
            );
        }

        previewRenderer =
            previewObject.AddComponent<SpriteRenderer>();

        previewRenderer.sprite =
            sprite;

        Color color =
            Color.white;

        color.a =
            previewAlpha;

        previewRenderer.color =
            color;

        if (prefabRenderer != null)
        {
            previewRenderer.sortingLayerID =
                prefabRenderer.sortingLayerID;

            previewRenderer.sortingOrder =
                prefabRenderer.sortingOrder +
                previewSortingOrderOffset;

            previewRenderer.flipX =
                prefabRenderer.flipX;

            previewRenderer.flipY =
                prefabRenderer.flipY;
        }

        float scaleMultiplier =
            gameplayModifiers != null
                ? gameplayModifiers
                    .IngredientScale
                : 1f;

        previewObject.transform.localScale =
            selectedIngredient
                .prefab
                .transform
                .localScale *
            scaleMultiplier;

        previewObject.SetActive(false);
    }

    private void ShowPreview()
    {
        if (previewObject != null)
        {
            previewObject.SetActive(
                true
            );
        }
    }

    private void HidePreview()
    {
        if (previewObject != null)
        {
            previewObject.SetActive(
                false
            );
        }
    }

    private void DestroyPreview()
    {
        if (previewObject != null)
        {
            Destroy(
                previewObject
            );

            previewObject = null;
            previewRenderer = null;
        }
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
                previewObject.transform.position,
                rotation,
                ingredientContainer
            );

        float scaleMultiplier =
            gameplayModifiers != null
                ? gameplayModifiers
                    .IngredientScale
                : 1f;

        spawned.transform.localScale *=
            scaleMultiplier;

        Ingredient ingredient =
            spawned.GetComponent<Ingredient>();

        if (ingredient != null)
        {
            ingredient.Initialize(
                selectedIngredient
            );
        }

        dropsThisChallenge++;

        selectedIngredient = null;

        previewRotation = 0f;

        DestroyPreview();

        draftManager?.IngredientWasDropped();
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
            gameplayModifiers.DropLimit > 0 &&
            dropsThisChallenge >=
                gameplayModifiers.DropLimit;
    }
}