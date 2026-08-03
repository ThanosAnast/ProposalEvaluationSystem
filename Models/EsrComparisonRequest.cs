namespace ProposalEvaluationSystem.Models;

public sealed class EsrComparisonRequest
{
    public required EvaluationResult IndependentEvaluation { get; init; }

    public required ReferenceEvaluation ReferenceEvaluation { get; init; }

    public required EvaluationProfile Profile { get; init; }

    public required string EsrText { get; init; }
}
