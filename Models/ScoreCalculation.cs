namespace ProposalEvaluationSystem.Models;

public sealed class ScoreCalculation
{
    public decimal TotalScore { get; init; }

    public IReadOnlyList<CriterionScoreCalculation> Criteria { get; init; } = [];

    public required ThresholdAssessment ThresholdAssessment { get; init; }
}
