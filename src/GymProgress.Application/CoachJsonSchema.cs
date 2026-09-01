namespace GymProgress.Application;

public static class CoachJsonSchema
{
    public const string Name = "workout_recommendation";

    public const string Schema = """
        {
          "type": "object",
          "properties": {
            "workoutType": { "type": "string" },
            "exercises": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "exerciseId": { "type": "string" },
                  "sets": { "type": "integer", "minimum": 1 },
                  "targetRepsMin": { "type": "integer", "minimum": 1 },
                  "targetRepsMax": { "type": "integer", "minimum": 1 },
                  "suggestedWeight": { "type": "number", "minimum": 0 },
                  "progression": {
                    "type": "string",
                    "enum": ["increase", "maintain", "decrease"]
                  },
                  "reason": { "type": "string" }
                },
                "required": [
                  "exerciseId",
                  "sets",
                  "targetRepsMin",
                  "targetRepsMax",
                  "suggestedWeight",
                  "progression",
                  "reason"
                ],
                "additionalProperties": false
              }
            },
            "coachNote": { "type": "string" }
          },
          "required": ["workoutType", "exercises", "coachNote"],
          "additionalProperties": false
        }
        """;
}
