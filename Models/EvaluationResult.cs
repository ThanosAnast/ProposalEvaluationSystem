using System.Text.Json.Serialization;

namespace ProposalEvaluationSystem.Models;

public sealed class EvaluationResult
{
    public string PromptTemplateUsed { get; set; } = string.Empty;

    public string EvaluationProfileId { get; set; } = string.Empty;

    public ProgrammeType ProgrammeType { get; set; }

    public string ModelName { get; set; } = string.Empty;

    public string PromptContentSha256 { get; set; } = string.Empty;

    public string InputFingerprint { get; set; } = string.Empty;

    public ScopeAssessment ScopeAssessment { get; set; } = new();

    public List<CriterionEvaluation> Criteria { get; set; } = [];

    public decimal TotalScore { get; set; }

    public List<CriterionScoreCalculation> ScoreBreakdown { get; set; } = [];

    public ThresholdAssessment ThresholdAssessment { get; set; } = new();

    public string OverallComment { get; set; } = string.Empty;

    public List<string> EvaluationLimitations { get; set; } = [];

    [JsonIgnore]
    public OpenAiResponseMetadata? ApiMetadata { get; set; }

    [JsonIgnore]
    public string GeneratedPrompt { get; set; } = string.Empty;
}
