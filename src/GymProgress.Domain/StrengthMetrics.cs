namespace GymProgress.Domain;

public static class StrengthMetrics
{
    public static decimal Volume(decimal weightKg, int reps)
    {
        if (weightKg < 0 || reps < 0)
        {
            return 0;
        }

        return weightKg * reps;
    }

    /// <summary>
    /// Epley: weight × (1 + reps / 30).
    /// </summary>
    public static decimal EstimatedOneRepMax(decimal weightKg, int reps)
    {
        if (weightKg <= 0 || reps <= 0)
        {
            return 0;
        }

        if (reps == 1)
        {
            return weightKg;
        }

        return decimal.Round(weightKg * (1m + reps / 30m), 2);
    }
}
