using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("Ingredients")]
    [SerializeField]
    private Transform ingredientContainer;

    [Header("Scoring")]
    [SerializeField]
    private float startingMult = 1f;

    private float startingMultBonus;

    public int Points
    {
        get;
        private set;
    }

    public float Mult
    {
        get;
        private set;
    }

    public int TotalScore
    {
        get;
        private set;
    }

    public event Action ScoreChanged;

    public void SetStartingMultBonus(
        float bonus
    )
    {
        startingMultBonus =
            bonus;

        RecalculateScore();
    }

    public void RecalculateScore()
    {
        Ingredient[] ingredients =
            GetScorableIngredients();

        int points = 0;

        float mult =
            startingMult +
            startingMultBonus;

        foreach (
            Ingredient ingredient
            in ingredients
        )
        {
            ScoreValue contribution =
                ingredient.EvaluateScore();

            points +=
                contribution.points;

            mult +=
                contribution.mult;
        }

        int previousPoints =
            Points;

        float previousMult =
            Mult;

        int previousTotal =
            TotalScore;

        Points =
            points;

        Mult =
            Mathf.Max(
                0f,
                mult
            );

        TotalScore =
            Mathf.RoundToInt(
                Points * Mult
            );

        if (
            previousPoints != Points ||
            !Mathf.Approximately(
                previousMult,
                Mult
            ) ||
            previousTotal != TotalScore
        )
        {
            ScoreChanged?.Invoke();
        }
    }

    private Ingredient[]
        GetScorableIngredients()
    {
        if (ingredientContainer != null)
        {
            return ingredientContainer
                .GetComponentsInChildren<Ingredient>(
                    false
                );
        }

        // Fallback for debugging if the
        // Inspector reference was forgotten.
        return FindObjectsByType<Ingredient>(
            FindObjectsSortMode.None
        );
    }

    private void LateUpdate()
    {
        RecalculateScore();
    }
}