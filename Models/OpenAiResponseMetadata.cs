namespace ProposalEvaluationSystem.Models;

public sealed class OpenAiResponseMetadata
{
    public string ResponseId { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int InputTokens { get; set; }

    public int OutputTokens { get; set; }

    public int TotalTokens { get; set; }

    public long DurationMilliseconds { get; set; }
}
