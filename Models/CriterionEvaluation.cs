namespace ProposalEvaluationSystem.Models;

public sealed class CriterionEvaluation
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal Score { get; set; }

    public List<string> Strengths { get; set; } = [];

    public List<string> Weaknesses { get; set; } = [];

    public List<string> Evidence { get; set; } = [];

    public string Assessment { get; set; } = string.Empty;
}
