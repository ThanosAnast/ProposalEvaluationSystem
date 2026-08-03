namespace ProposalEvaluationSystem.Services;

public class OpenAiOptions
{
    public string? ApiKey { get; set; }

    public string Model { get; set; } = "gpt-5.4-mini";

    public string Endpoint { get; set; } = "https://api.openai.com/v1/responses";

    public int MaxOutputTokens { get; set; } = 5000;

    public int TimeoutSeconds { get; set; } = 180;

    public int MaxAttempts { get; set; } = 3;

    public int InitialRetryDelayMilliseconds { get; set; } = 500;

    public string? GetApiKey() =>
        string.IsNullOrWhiteSpace(ApiKey)
            ? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            : ApiKey;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(GetApiKey());
}
