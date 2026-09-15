using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class IngredientHoverController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private RunManager runManager;
    [SerializeField] private ViewController viewController;

    [Header("Detection")]
    [SerializeField]
    private LayerMask ingredientLayerMask = ~0;

    [Header("Highlight")]
    [SerializeField]
    private Color hoveredColor =
        new Color(
            1f,
            0.85f,
            0.1f,
            1f
        );

    [SerializeField]
    private Color touchingColor =
        new Color(
            1f,
            0.55f,
            0.1f,
            1f
        );

    [SerializeField]
    private float hoveredWidth = 0.07f;

    [SerializeField]
    private float touchingWidth = 0.05f;

    private Ingredient hoveredIngredient;

    private readonly HashSet<Ingredient>
        highlightedIngredients =
            new HashSet<Ingredient>();

    private void Awake()
    {
        if (gameplayCamera == null)
        {
            gameplayCamera =
                Camera.main;
        }

        if (runManager == null)
        {
            runManager =
                FindFirstObjectByType<RunManager>();
        }

        if (viewController == null)
        {
            viewController =
                FindFirstObjectByType<ViewController>();
        }
    }

    private void Update()
    {
        if (!CanHover())
        {
            ClearHighlights();
            return;
        }

        Mouse mouse =
            Mouse.current;

        if (
            mouse == null ||
            gameplayCamera == null
        )
        {
            ClearHighlights();
            return;
        }

        if (
            EventSystem.current != null &&
            EventSystem.current
                .IsPointerOverGameObject()
        )
        {
            ClearHighlights();
            return;
        }

        Vector3 mouseWorld =
            gameplayCamera.ScreenToWorldPoint(
                mouse.position.ReadValue()
            );

        mouseWorld.z = 0f;

        Ingredient ingredient =
            FindIngredientAtPoint(
                mouseWorld
            );

        if (
            ingredient ==
            hoveredIngredient
        )
        {
            return;
        }

        hoveredIngredient =
            ingredient;

        RefreshHighlights();
    }

    private Ingredient FindIngredientAtPoint(
        Vector2 point
    )
    {
        Collider2D[] hits =
            Physics2D.OverlapPointAll(
                point,
                ingredientLayerMask
            );

        Ingredient bestIngredient =
            null;

        int bestSortingOrder =
            int.MinValue;

        float bestDistance =
            float.MaxValue;

        foreach (
            Collider2D hit
            in hits
        )
        {
            if (hit == null)
            {
                continue;
            }

            Ingredient ingredient =
                hit.GetComponentInParent<
                    Ingredient
                >();

            if (ingredient == null)
            {
                continue;
            }

            SpriteRenderer renderer =
                ingredient
                    .GetComponentInChildren<
                        SpriteRenderer
                    >();

            int sortingOrder =
                renderer != null
                    ? renderer.sortingOrder
                    : 0;

            float distance =
                Vector2.Distance(
                    point,
                    ingredient
                        .transform
                        .position
                );

            bool better =
                sortingOrder >
                bestSortingOrder;

            if (
                sortingOrder ==
                    bestSortingOrder &&
                distance <
                    bestDistance
            )
            {
                better = true;
            }

            if (!better)
            {
                continue;
            }

            bestIngredient =
                ingredient;

            bestSortingOrder =
                sortingOrder;

            bestDistance =
                distance;
        }

        return bestIngredient;
    }

    private void RefreshHighlights()
    {
        // Hide the previous outlines,
        // but DO NOT clear hoveredIngredient.
        HideHighlightLines();

        if (hoveredIngredient == null)
        {
            return;
        }

        HighlightIngredient(
            hoveredIngredient,
            hoveredColor,
            hoveredWidth
        );

        foreach (
            Ingredient touching
            in hoveredIngredient
                .TouchingIngredients
        )
        {
            if (touching == null)
            {
                continue;
            }

            HighlightIngredient(
                touching,
                touchingColor,
                touchingWidth
            );
        }
    }

    private void HighlightIngredient(
        Ingredient ingredient,
        Color color,
        float width
    )
    {
        if (ingredient == null)
        {
            return;
        }

        IngredientHighlight highlight =
            ingredient.GetComponent<
                IngredientHighlight
            >();

        if (highlight == null)
        {
            highlight =
                ingredient.gameObject
                    .AddComponent<
                        IngredientHighlight
                    >();
        }

        highlight.Show(
            color,
            width
        );

        highlightedIngredients.Add(
            ingredient
        );
    }

    private void HideHighlightLines()
    {
        foreach (
            Ingredient ingredient
            in highlightedIngredients
        )
        {
            if (ingredient == null)
            {
                continue;
            }

            IngredientHighlight highlight =
                ingredient.GetComponent<
                    IngredientHighlight
                >();

            highlight?.Hide();
        }

        highlightedIngredients.Clear();
    }

    private void ClearHighlights()
    {
        HideHighlightLines();

        hoveredIngredient =
            null;
    }

    private bool CanHover()
    {
        if (
            runManager != null &&
            runManager.State !=
                RunState.Assembly
        )
        {
            return false;
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
            return false;
        }

        return true;
    }

    private void OnDisable()
    {
        ClearHighlights();
    }
}