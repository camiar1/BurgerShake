using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class IngredientDropper : MonoBehaviour
{
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private ViewController viewController;
    [SerializeField] private float minX = -4f;
    [SerializeField] private float maxX = 4f;
    [SerializeField] private float dropY = 4f;
    [SerializeField] private Transform ingredientContainer;

    private IngredientDefinition selectedIngredient;
    private IngredientDraftManager draftManager;

    public bool HasIngredient => selectedIngredient != null;

    public void Initialize(IngredientDraftManager manager)
    {
        draftManager = manager;
    }

    public void SetIngredient(IngredientDefinition ingredient)
    {
        selectedIngredient = ingredient;
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
    }

    private void Update()
    {
        if (selectedIngredient == null || gameplayCamera == null)
        {
            return;
        }

        if (viewController != null &&
            (viewController.IsSliding || viewController.CurrentView != ViewController.FoodTruckView.Assembly))
        {
            return;
        }

        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        Vector3 mousePosition = gameplayCamera.ScreenToWorldPoint(mouse.position.ReadValue());
        transform.position = new Vector3(Mathf.Clamp(mousePosition.x, minX, maxX), dropY, transform.position.z);

        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            DropIngredient();
        }
    }

    private void DropIngredient()
    {
        if (selectedIngredient == null || selectedIngredient.prefab == null)
        {
            return;
        }

        GameObject spawned = Instantiate(selectedIngredient.prefab, transform.position, Quaternion.identity, ingredientContainer);
        Ingredient ingredient = spawned.GetComponent<Ingredient>();

        if (ingredient != null)
        {
            ingredient.Initialize(selectedIngredient);
        }

        selectedIngredient = null;
        draftManager?.IngredientWasDropped();
    }
}
