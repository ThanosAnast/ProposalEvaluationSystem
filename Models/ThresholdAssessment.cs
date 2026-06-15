namespace ProposalEvaluationSystem.Models;

public class ThresholdAssessment
{
    public decimal TotalScore { get; set; }

    public decimal RequiredTotalScore { get; set; }

    public bool IndividualThresholdsMet { get; set; }

    public string OverallResult { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;
}
