namespace ProposalEvaluationSystem.Models;

public sealed class CriterionEvaluation
{
    public string CriterionId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal Score { get; set; }

    public string Summary { get; set; } = string.Empty;

    public List<string> Strengths { get; set; } = [];

    public List<string> Shortcomings { get; set; } = [];

    public List<string> Evidence { get; set; } = [];

    public List<string> Limitations { get; set; } = [];
}
