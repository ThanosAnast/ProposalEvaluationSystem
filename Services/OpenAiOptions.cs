namespace ProposalEvaluationSystem.Services;

public class OpenAiOptions
{
    public const string DefaultModel = "gpt-5.4-mini-2026-03-17";

    public const string DefaultReasoningEffort = "medium";

    public const int DefaultMaxOutputTokens = 5000;

    public string? ApiKey { get; set; }

    public string Model { get; set; } = DefaultModel;

    public string ReasoningEffort { get; set; } = DefaultReasoningEffort;

    public string Endpoint { get; set; } = "https://api.openai.com/v1/responses";

    public int MaxOutputTokens { get; set; } = DefaultMaxOutputTokens;

    public string ComparisonModel { get; set; } = DefaultModel;

    public string ComparisonReasoningEffort { get; set; } = DefaultReasoningEffort;

    public int ComparisonMaxOutputTokens { get; set; } = DefaultMaxOutputTokens;

    public int TimeoutSeconds { get; set; } = 180;

    public int MaxAttempts { get; set; } = 3;

    public int InitialRetryDelayMilliseconds { get; set; } = 500;

    public string? GetApiKey() =>
        string.IsNullOrWhiteSpace(ApiKey)
            ? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            : ApiKey;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(GetApiKey());
}
