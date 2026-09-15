using System;
using System.Collections.Generic;
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

    private readonly List<IngredientScoreStep>
        lastBreakdown =
            new List<IngredientScoreStep>();

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

    public float StartingMultValue =>
        Mathf.Max(
            0f,
            startingMult +
            startingMultBonus
        );

    public IReadOnlyList<IngredientScoreStep>
        LastBreakdown =>
            lastBreakdown;

    public event Action ScoreChanged;

    public void SetStartingMultBonus(
        float bonus
    )
    {
        startingMultBonus =
            bonus;
    }

    public void ResetScore()
    {
        lastBreakdown.Clear();

        Points =
            0;

        Mult =
            StartingMultValue;

        TotalScore =
            0;

        ScoreChanged?.Invoke();
    }

    public void CalculateFinalScore()
    {
        lastBreakdown.Clear();

        Ingredient[] ingredients =
            GetScorableIngredientsInOrder();

        int runningPoints =
            0;

        float rawRunningMult =
            startingMult +
            startingMultBonus;

        int runningTotal =
            0;

        foreach (
            Ingredient ingredient
            in ingredients
        )
        {
            if (
                ingredient == null ||
                ingredient.Definition == null ||
                ingredient.Definition.scoringRules ==
                    null
            )
            {
                continue;
            }

            foreach (
                IngredientScoringRule rule
                in ingredient.Definition.scoringRules
            )
            {
                if (rule == null)
                {
                    continue;
                }

                int pointsBefore =
                    runningPoints;

                float multBefore =
                    Mathf.Max(
                        0f,
                        rawRunningMult
                    );

                int totalBefore =
                    runningTotal;

                ScoreValue contribution =
                    rule.Evaluate(
                        ingredient
                    );

                runningPoints +=
                    contribution.points;

                rawRunningMult +=
                    contribution.mult;

                float multAfter =
                    Mathf.Max(
                        0f,
                        rawRunningMult
                    );

                runningTotal =
                    Mathf.RoundToInt(
                        runningPoints *
                        multAfter
                    );

                IngredientScoreStep step =
                    new IngredientScoreStep(
                        ingredient,
                        rule,
                        contribution,
                        pointsBefore,
                        runningPoints,
                        multBefore,
                        multAfter,
                        totalBefore,
                        runningTotal
                    );

                lastBreakdown.Add(
                    step
                );
            }
        }

        Points =
            runningPoints;

        Mult =
            Mathf.Max(
                0f,
                rawRunningMult
            );

        TotalScore =
            Mathf.RoundToInt(
                Points *
                Mult
            );

        ScoreChanged?.Invoke();
    }

    public void RecalculateScore()
    {
        CalculateFinalScore();
    }

    private Ingredient[]
        GetScorableIngredientsInOrder()
    {
        if (ingredientContainer != null)
        {
            List<Ingredient> ingredients =
                new List<Ingredient>();

            for (
                int i = 0;
                i <
                ingredientContainer.childCount;
                i++
            )
            {
                Transform child =
                    ingredientContainer
                        .GetChild(i);

                Ingredient ingredient =
                    child.GetComponent<
                        Ingredient
                    >();

                if (ingredient == null)
                {
                    ingredient =
                        child
                            .GetComponentInChildren<
                                Ingredient
                            >(
                                false
                            );
                }

                if (ingredient != null)
                {
                    ingredients.Add(
                        ingredient
                    );
                }
            }

            return ingredients.ToArray();
        }

        return FindObjectsByType<
            Ingredient
        >(
            FindObjectsSortMode.None
        );
    }
}