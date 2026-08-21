namespace ProposalEvaluationSystem.Models;

public sealed class CriterionScoreCalculation
{
    public string CriterionId { get; set; } = string.Empty;

    public string CriterionName { get; set; } = string.Empty;

    public decimal RawScore { get; set; }

    public decimal ScoreMinimum { get; set; }

    public decimal ScoreMaximum { get; set; }

    public decimal? ScoreIncrement { get; set; }

    public decimal? WeightPercentage { get; set; }

    public decimal ContributionToTotal { get; set; }

    public decimal? Threshold { get; set; }

    public bool? ThresholdMet { get; set; }
}
