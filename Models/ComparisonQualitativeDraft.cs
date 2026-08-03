namespace ProposalEvaluationSystem.Models;

public sealed class ComparisonQualitativeDraft
{
    public List<string> SharedStrengths { get; set; } = [];

    public List<string> SharedWeaknesses { get; set; } = [];

    public List<string> FindingsDetectedOnlyByLlm { get; set; } = [];

    public List<string> FindingsPresentOnlyInEsr { get; set; } = [];

    public string OverallComparisonSummary { get; set; } = string.Empty;

    public List<string> ComparisonLimitations { get; set; } = [];
}
