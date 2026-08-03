namespace ProposalEvaluationSystem.Models;

public sealed class CriterionDefinition
{
    public required string Id { get; init; }

    public required string DisplayName { get; init; }

    public decimal Threshold { get; init; }
}
