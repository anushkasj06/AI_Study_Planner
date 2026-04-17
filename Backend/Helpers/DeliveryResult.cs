namespace AIStudyPlanner.Api.Helpers;

public sealed class DeliveryResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int AttemptedTargets { get; init; }
    public int DeliveredTargets { get; init; }

    public static DeliveryResult Ok(string message, int attemptedTargets = 1, int deliveredTargets = 1) =>
        new()
        {
            Success = true,
            Message = message,
            AttemptedTargets = attemptedTargets,
            DeliveredTargets = deliveredTargets
        };

    public static DeliveryResult Fail(string message, int attemptedTargets = 1, int deliveredTargets = 0) =>
        new()
        {
            Success = false,
            Message = message,
            AttemptedTargets = attemptedTargets,
            DeliveredTargets = deliveredTargets
        };
}
