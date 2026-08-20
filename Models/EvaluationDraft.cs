namespace ProposalEvaluationSystem.Models;

public sealed class EvaluationDraft
{
    public ScopeAssessment ScopeAssessment { get; set; } = new();

    public List<CriterionEvaluation> Criteria { get; set; } = [];

    public string OverallComment { get; set; } = string.Empty;

    public List<string> EvaluationLimitations { get; set; } = [];
}
