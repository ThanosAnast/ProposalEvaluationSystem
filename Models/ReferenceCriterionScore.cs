namespace ProposalEvaluationSystem.Models;

public sealed class ReferenceCriterionScore
{
    public string CriterionId { get; set; } = string.Empty;

    public decimal OfficialScore { get; set; }
}
