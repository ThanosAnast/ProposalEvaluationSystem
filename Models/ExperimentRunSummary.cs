namespace ProposalEvaluationSystem.Models;

public sealed class ExperimentRunSummary
{
    public string DatasetId { get; set; } = string.Empty;

    public string RunId { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string EvaluationProfileId { get; set; } = string.Empty;

    public string ModelName { get; set; } = string.Empty;

    public decimal TotalScore { get; set; }

    public string ThresholdResult { get; set; } = string.Empty;

    public bool HasEsrComparison { get; set; }

    public decimal? OfficialTotalScore { get; set; }

    public decimal? TotalScoreDifference { get; set; }

    public bool? ThresholdAgreement { get; set; }
}
