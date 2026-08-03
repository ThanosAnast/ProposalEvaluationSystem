namespace ProposalEvaluationSystem.Models;

public sealed class ComparisonQualitativeDraft
{
    public List<CriterionComparisonQualitativeDraft> Criteria { get; set; } = [];

    public string OverallComparisonSummary { get; set; } = string.Empty;

    public List<string> ComparisonLimitations { get; set; } = [];
}
