namespace AIStudyPlanner.Api.Helpers;

public sealed class GroqOptions
{
    public const string SectionName = "Groq";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "llama-3.1-70b-versatile";
    public string Endpoint { get; set; } = "https://api.groq.com/openai/v1/chat/completions";
    public int RequestTimeoutSeconds { get; set; } = 25;
}
