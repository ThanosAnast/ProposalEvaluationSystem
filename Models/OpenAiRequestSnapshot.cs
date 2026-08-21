namespace ProposalEvaluationSystem.Models;

public sealed class OpenAiRequestSnapshot
{
    public string ConfiguredModel { get; set; } = string.Empty;

    public string ReasoningEffort { get; set; } = string.Empty;

    public int MaxOutputTokens { get; set; }

    public int TimeoutSeconds { get; set; }

    public int MaxAttempts { get; set; }

    public int InitialRetryDelayMilliseconds { get; set; }

    public bool StoreResponse { get; set; }

    public string Endpoint { get; set; } = string.Empty;
}
