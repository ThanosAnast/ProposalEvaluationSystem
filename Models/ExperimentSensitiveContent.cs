namespace ProposalEvaluationSystem.Models;

public sealed class ExperimentSensitiveContent
{
    public string CallText { get; set; } = string.Empty;

    public string ProposalText { get; set; } = string.Empty;

    public string? EsrText { get; set; }

    public string GeneratedPrompt { get; set; } = string.Empty;
}
