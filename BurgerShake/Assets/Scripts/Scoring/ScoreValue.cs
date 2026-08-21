using System;

[Serializable]
public struct ScoreValue
{
    public int points;
    public float mult;

    public ScoreValue(int points, float mult)
    {
        this.points = points;
        this.mult = mult;
    }

    public static ScoreValue operator +(ScoreValue a, ScoreValue b)
    {
        return new ScoreValue(a.points + b.points, a.mult + b.mult);
    }
}
