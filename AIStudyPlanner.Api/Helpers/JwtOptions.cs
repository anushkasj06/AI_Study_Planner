namespace AIStudyPlanner.Api.Helpers;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = "ChangeThisToA32CharOrLongerJwtSecretKey123!";
    public string Issuer { get; set; } = "AIStudyPlannerApi";
    public string Audience { get; set; } = "AIStudyPlannerClient";
    public int ExpireMinutes { get; set; } = 120;
}
