namespace ProposalEvaluationSystem.Models;

public sealed class CriterionComparisonQualitativeDraft
{
    public string CriterionId { get; set; } = string.Empty;

    public List<string> SharedStrengths { get; set; } = [];

    public List<string> SharedWeaknesses { get; set; } = [];

    public List<string> FindingsDetectedOnlyByLlm { get; set; } = [];

    public List<string> FindingsPresentOnlyInEsr { get; set; } = [];

    public string Summary { get; set; } = string.Empty;
}
