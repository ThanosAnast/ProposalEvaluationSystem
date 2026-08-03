namespace ProposalEvaluationSystem.Models;

public sealed class EvaluationComparisonResult
{
    public List<CriterionComparison> Criteria { get; set; } = [];

    public decimal OfficialTotalScore { get; set; }

    public decimal LlmTotalScore { get; set; }

    /// <summary>LLM total score minus official ESR total score.</summary>
    public decimal TotalScoreDifference { get; set; }

    public bool OfficialThresholdMet { get; set; }

    public bool LlmThresholdMet { get; set; }

    public bool ThresholdAgreement { get; set; }

    public List<string> SharedStrengths { get; set; } = [];

    public List<string> SharedWeaknesses { get; set; } = [];

    public List<string> FindingsDetectedOnlyByLlm { get; set; } = [];

    public List<string> FindingsPresentOnlyInEsr { get; set; } = [];

    public string OverallComparisonSummary { get; set; } = string.Empty;

    public List<string> ComparisonLimitations { get; set; } = [];
}
