namespace ProposalEvaluationSystem.Models;

public sealed class CriterionDefinition
{
    public required string Id { get; init; }

    public required string DisplayName { get; init; }

    public decimal ScoreMinimum { get; init; }

    public decimal ScoreMaximum { get; init; }

    public decimal? ScoreIncrement { get; init; }

    public decimal? Threshold { get; init; }

    public decimal? WeightPercentage { get; init; }
}
