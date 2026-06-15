namespace ProposalEvaluationSystem.Models;

public class EvaluationResult
{
    public string PromptTemplateUsed { get; set; } = string.Empty;

    public ProgrammeType ProgrammeType { get; set; }

    public EvaluationLevel EvaluationLevel { get; set; }

    public string ExecutiveSummary { get; set; } = string.Empty;

    public CriterionEvaluation Excellence { get; set; } = new() { Name = "Excellence" };

    public CriterionEvaluation Impact { get; set; } = new() { Name = "Impact" };

    public CriterionEvaluation Implementation { get; set; } = new() { Name = "Quality and Efficiency of Implementation" };

    public decimal TotalScore { get; set; }

    public ThresholdAssessment ThresholdAssessment { get; set; } = new();

    public string FinalComment { get; set; } = string.Empty;

    public string ConfidenceLevel { get; set; } = string.Empty;

    public List<string> Limitations { get; set; } = [];

    public string GeneratedPrompt { get; set; } = string.Empty;

    public string RealEvaluationComparisonPlaceholder { get; set; } = string.Empty;
}
