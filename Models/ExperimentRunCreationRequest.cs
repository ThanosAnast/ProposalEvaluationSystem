namespace ProposalEvaluationSystem.Models;

public sealed class ExperimentRunCreationRequest
{
    public required string DatasetId { get; init; }

    public required EvaluationProfile Profile { get; init; }

    public required ProcessedDocument CallDocument { get; init; }

    public required ProcessedDocument ProposalDocument { get; init; }

    public ProcessedDocument? EsrDocument { get; init; }

    public required string PromptContent { get; init; }

    public required string GeneratedPrompt { get; init; }

    public required EvaluationResult IndependentEvaluation { get; init; }

    public EvaluationComparisonResult? ComparisonResult { get; init; }

    public TimeSpan ExecutionDuration { get; init; }

    public IReadOnlyCollection<string> Warnings { get; init; } = [];

    public IReadOnlyCollection<string> Errors { get; init; } = [];
}
