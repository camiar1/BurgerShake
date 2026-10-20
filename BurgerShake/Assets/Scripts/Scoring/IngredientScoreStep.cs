public sealed class IngredientScoreStep
{
    public Ingredient ingredient;

    public IngredientScoringRule rule;

    public ScoreValue contribution;

    public int pointsBefore;
    public int pointsAfter;

    public float multBefore;
    public float multAfter;

    public int totalBefore;
    public int totalAfter;

    public int TotalDelta =>
        totalAfter - totalBefore;

    public IngredientScoreStep(
        Ingredient ingredient,
        IngredientScoringRule rule,
        ScoreValue contribution,
        int pointsBefore,
        int pointsAfter,
        float multBefore,
        float multAfter,
        int totalBefore,
        int totalAfter
    )
    {
        this.ingredient =
            ingredient;

        this.rule =
            rule;

        this.contribution =
            contribution;

        this.pointsBefore =
            pointsBefore;

        this.pointsAfter =
            pointsAfter;

        this.multBefore =
            multBefore;

        this.multAfter =
            multAfter;

        this.totalBefore =
            totalBefore;

        this.totalAfter =
            totalAfter;
    }
}