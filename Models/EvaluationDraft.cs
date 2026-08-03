namespace ProposalEvaluationSystem.Models;

public sealed class EvaluationDraft
{
    public string ExecutiveSummary { get; set; } = string.Empty;

    public List<CriterionEvaluation> Criteria { get; set; } = [];

    public string FinalComment { get; set; } = string.Empty;

    public string ConfidenceLevel { get; set; } = string.Empty;

    public List<string> Limitations { get; set; } = [];
}
