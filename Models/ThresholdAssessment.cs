namespace ProposalEvaluationSystem.Models;

public sealed class ThresholdAssessment
{
    public decimal TotalScore { get; set; }

    public decimal RequiredTotalScore { get; set; }

    public decimal MaximumTotalScore { get; set; }

    public bool IndividualThresholdsMet { get; set; }

    public bool OverallThresholdMet { get; set; }

    public bool Passed { get; set; }

    public string OverallResult { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;
}
