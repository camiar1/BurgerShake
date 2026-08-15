using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private float startingMult = 1f;

    public int Points { get; private set; }
    public float Mult { get; private set; }
    public int TotalScore { get; private set; }

    public void RecalculateScore()
    {
        Ingredient[] ingredients = FindObjectsByType<Ingredient>(FindObjectsSortMode.None);

        int points = 0;
        float mult = startingMult;

        foreach (Ingredient ingredient in ingredients)
        {
            points += ingredient.CalculatePoints();
            mult += ingredient.CalculateMult();
        }

        Points = points;
        Mult = Mathf.Max(0f, mult);
        TotalScore = Mathf.RoundToInt(Points * Mult);
    }

    private void LateUpdate()
    {
        RecalculateScore();
    }
}
