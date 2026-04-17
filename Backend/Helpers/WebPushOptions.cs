namespace AIStudyPlanner.Api.Helpers;

public sealed class WebPushOptions
{
    public const string SectionName = "WebPush";

    public string Subject { get; set; } = "mailto:admin@example.com";
    public string PublicKey { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Subject) &&
        !string.IsNullOrWhiteSpace(PublicKey) &&
        !string.IsNullOrWhiteSpace(PrivateKey);
}
