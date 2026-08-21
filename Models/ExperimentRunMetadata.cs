namespace ProposalEvaluationSystem.Models;

public sealed class ExperimentRunMetadata
{
    public string RunId { get; set; } = string.Empty;

    public string DatasetId { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string EvaluationProfileId { get; set; } = string.Empty;

    public ProgrammeType ProgrammeType { get; set; }

    public string ModelName { get; set; } = string.Empty;

    public string PromptFileName { get; set; } = string.Empty;

    public string PromptContentSha256 { get; set; } = string.Empty;

    public ExperimentDocumentMetadata Call { get; set; } = new();

    public ExperimentDocumentMetadata Proposal { get; set; } = new();

    public ExperimentDocumentMetadata? Esr { get; set; }

    public string InputFingerprint { get; set; } = string.Empty;

    public long ExecutionDurationMilliseconds { get; set; }

    public string ApplicationVersion { get; set; } = string.Empty;

    public string GitCommitSha { get; set; } = string.Empty;

    public string ExperimentSchemaVersion { get; set; } = string.Empty;

    public EvaluationProfileSnapshot? ProfileSnapshot { get; set; }

    public PromptTemplateSnapshot? PromptTemplateSnapshot { get; set; }

    public OpenAiRequestSnapshot? OpenAiRequestSnapshot { get; set; }

    public OpenAiResponseMetadata? EvaluationApiMetadata { get; set; }

    public OpenAiResponseMetadata? ComparisonApiMetadata { get; set; }
}
