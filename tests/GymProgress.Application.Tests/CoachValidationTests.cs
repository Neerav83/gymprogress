using GymProgress.Application;
using GymProgress.Application.Contracts;
using GymProgress.Domain;

namespace GymProgress.Application.Tests;

public class AiResponseParserTests
{
    [Fact]
    public void Parses_valid_json()
    {
        const string json = """
            {
              "workoutType": "Push",
              "coachNote": "Keep it simple.",
              "exercises": [
                {
                  "exerciseId": "aaaaaaaa-0000-4000-8000-000000000001",
                  "sets": 3,
                  "targetRepsMin": 8,
                  "targetRepsMax": 10,
                  "suggestedWeight": 25,
                  "progression": "maintain",
                  "reason": "Last session was solid."
                }
              ]
            }
            """;

        var parsed = AiResponseParser.ParseRecommendation(json);
        Assert.Equal("Push", parsed.WorkoutType);
        var exercise = Assert.Single(parsed.Exercises);
        Assert.Equal(25, exercise.SuggestedWeight);
        Assert.Equal("maintain", exercise.Progression);
    }

    [Fact]
    public void Parses_json_wrapped_in_markdown()
    {
        const string wrapped = """
            ```json
            {"workoutType":"Legs","exercises":[],"coachNote":"Rest if needed."}
            ```
            """;

        var parsed = AiResponseParser.ParseRecommendation(wrapped);
        Assert.Equal("Legs", parsed.WorkoutType);
        Assert.Equal("Rest if needed.", parsed.CoachNote);
    }

    [Fact]
    public void Rejects_invalid_json()
    {
        Assert.Throws<CoachInvalidResponseException>(() => AiResponseParser.ParseRecommendation("{not json"));
    }

    [Fact]
    public void Rejects_empty_content()
    {
        Assert.Throws<CoachInvalidResponseException>(() => AiResponseParser.ParseRecommendation("  "));
    }
}

public class CoachRecommendationValidatorTests
{
    private static readonly Guid ChestPressId = Guid.Parse("aaaaaaaa-0000-4000-8000-000000000010");

    private static IReadOnlyDictionary<Guid, Exercise> Catalog() => new Dictionary<Guid, Exercise>
    {
        [ChestPressId] = new Exercise
        {
            Id = ChestPressId,
            Name = "Chest Press",
            MuscleGroups = ["chest"],
            Equipment = Equipment.Machine
        }
    };

    [Fact]
    public void Accepts_valid_recommendation()
    {
        var raw = new AiWorkoutRecommendation(
            "Push",
            [Exercise("maintain")],
            "A focused push day.");

        var result = CoachRecommendationValidator.Validate(raw, Catalog());
        var exercise = Assert.Single(result.Exercises);
        Assert.Equal("Chest Press", exercise.ExerciseName);
        Assert.Equal("maintain", exercise.Progression);
    }

    [Theory]
    [InlineData("increase")]
    [InlineData("maintain")]
    [InlineData("decrease")]
    [InlineData("INCREASE")]
    public void Accepts_allowed_progression_values(string progression)
    {
        var raw = new AiWorkoutRecommendation("Push", [Exercise(progression)], "Note");
        var result = CoachRecommendationValidator.Validate(raw, Catalog());
        Assert.Equal(progression.ToLowerInvariant(), result.Exercises[0].Progression);
    }

    [Fact]
    public void Rejects_unknown_exercise_id()
    {
        var raw = new AiWorkoutRecommendation(
            "Push",
            [new AiRecommendedExercise(Guid.NewGuid().ToString(), 3, 8, 10, 25, "maintain", "Guess")],
            "Note");

        var exception = Assert.Throws<CoachInvalidResponseException>(
            () => CoachRecommendationValidator.Validate(raw, Catalog()));
        Assert.Contains("okänd", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Rejects_invalid_progression()
    {
        var raw = new AiWorkoutRecommendation("Push", [Exercise("explode")], "Note");
        Assert.Throws<CoachInvalidResponseException>(() => CoachRecommendationValidator.Validate(raw, Catalog()));
    }

    [Fact]
    public void Rejects_when_min_reps_exceed_max()
    {
        var raw = new AiWorkoutRecommendation(
            "Push",
            [new AiRecommendedExercise(ChestPressId.ToString(), 3, 12, 8, 25, "maintain", "Bad range")],
            "Note");

        Assert.Throws<CoachInvalidResponseException>(() => CoachRecommendationValidator.Validate(raw, Catalog()));
    }

    [Fact]
    public void Rejects_negative_weight()
    {
        var raw = new AiWorkoutRecommendation(
            "Push",
            [new AiRecommendedExercise(ChestPressId.ToString(), 3, 8, 10, -5, "maintain", "Bad weight")],
            "Note");

        Assert.Throws<CoachInvalidResponseException>(() => CoachRecommendationValidator.Validate(raw, Catalog()));
    }

    [Fact]
    public void Rejects_zero_sets()
    {
        var raw = new AiWorkoutRecommendation(
            "Push",
            [new AiRecommendedExercise(ChestPressId.ToString(), 0, 8, 10, 25, "maintain", "No sets")],
            "Note");

        Assert.Throws<CoachInvalidResponseException>(() => CoachRecommendationValidator.Validate(raw, Catalog()));
    }

    private static AiRecommendedExercise Exercise(string progression) =>
        new(ChestPressId.ToString(), 3, 8, 10, 25, progression, "Keep the load.");
}
