namespace ProposalEvaluationSystem.Models;

public class CriterionEvaluation
{
    public string Name { get; set; } = string.Empty;

    public decimal Score { get; set; }

    public List<string> Strengths { get; set; } = [];

    public List<string> Weaknesses { get; set; } = [];

    public List<string> Evidence { get; set; } = [];

    public string Assessment { get; set; } = string.Empty;
}
