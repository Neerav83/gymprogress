namespace GymProgress.Application;

public sealed class CoachUnavailableException(string message, Exception? inner = null)
    : Exception(message, inner);

public sealed class CoachInvalidResponseException(string message, Exception? inner = null)
    : Exception(message, inner);
