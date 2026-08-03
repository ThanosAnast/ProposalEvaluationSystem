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

    public string ExecutiveSummary { get; set; } = string.Empty;

    public List<CriterionEvaluation> Criteria { get; set; } = [];

    public decimal TotalScore { get; set; }

    public ThresholdAssessment ThresholdAssessment { get; set; } = new();

    public string FinalComment { get; set; } = string.Empty;

    public string ConfidenceLevel { get; set; } = string.Empty;

    public List<string> Limitations { get; set; } = [];

    [JsonIgnore]
    public OpenAiResponseMetadata? ApiMetadata { get; set; }

    [JsonIgnore]
    public string GeneratedPrompt { get; set; } = string.Empty;
}
