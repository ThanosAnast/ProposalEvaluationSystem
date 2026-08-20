namespace ProposalEvaluationSystem.Models;

public sealed class ScopeAssessment
{
    public string Status { get; set; } = string.Empty;

    public string Rationale { get; set; } = string.Empty;

    public List<string> Evidence { get; set; } = [];
}
