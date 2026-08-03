namespace ProposalEvaluationSystem.Models;

public sealed class ExperimentDocumentMetadata
{
    public string FileName { get; set; } = string.Empty;

    public string ContentSha256 { get; set; } = string.Empty;
}
