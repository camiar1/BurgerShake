using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private float startingMult = 1f;

    public int Points { get; private set; }
    public float Mult { get; private set; }
    public int TotalScore { get; private set; }

    public event Action ScoreChanged;

    public void RecalculateScore()
    {
        Ingredient[] ingredients = FindObjectsByType<Ingredient>(FindObjectsSortMode.None);

        int points = 0;
        float mult = startingMult;

        foreach (Ingredient ingredient in ingredients)
        {
            ScoreValue contribution = ingredient.EvaluateScore();
            points += contribution.points;
            mult += contribution.mult;
        }

        int previousPoints = Points;
        float previousMult = Mult;
        int previousTotal = TotalScore;

        Points = points;
        Mult = Mathf.Max(0f, mult);
        TotalScore = Mathf.RoundToInt(Points * Mult);

        if (previousPoints != Points || !Mathf.Approximately(previousMult, Mult) || previousTotal != TotalScore)
        {
            ScoreChanged?.Invoke();
        }
    }

    private void LateUpdate()
    {
        RecalculateScore();
    }
}
