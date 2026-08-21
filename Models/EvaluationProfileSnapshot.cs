namespace ProposalEvaluationSystem.Models;

public sealed class EvaluationProfileSnapshot
{
    public string Id { get; set; } = string.Empty;

    public ProgrammeType ProgrammeType { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string PromptTemplateFileName { get; set; } = string.Empty;

    public ScoringMode ScoringMode { get; set; }

    public decimal OverallThreshold { get; set; }

    public decimal MaximumTotal { get; set; }

    public List<EvaluationCriterionSnapshot> Criteria { get; set; } = [];
}

public sealed class EvaluationCriterionSnapshot
{
    public string Id { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public decimal ScoreMinimum { get; set; }

    public decimal ScoreMaximum { get; set; }

    public decimal? ScoreIncrement { get; set; }

    public decimal? Threshold { get; set; }

    public decimal? WeightPercentage { get; set; }
}
