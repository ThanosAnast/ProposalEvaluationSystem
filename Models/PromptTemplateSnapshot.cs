namespace ProposalEvaluationSystem.Models;

public sealed class PromptTemplateSnapshot
{
    public string FileName { get; set; } = string.Empty;

    public string ContentSha256 { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}
