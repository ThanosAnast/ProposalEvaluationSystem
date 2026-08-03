namespace ProposalEvaluationSystem.Models;

public sealed class CriterionComparison
{
    public string CriterionId { get; set; } = string.Empty;

    public string CriterionName { get; set; } = string.Empty;

    public decimal OfficialScore { get; set; }

    public decimal LlmScore { get; set; }

    /// <summary>LLM score minus official ESR score.</summary>
    public decimal NumericDifference { get; set; }
}
