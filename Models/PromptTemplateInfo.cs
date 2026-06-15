namespace ProposalEvaluationSystem.Models;

public class PromptTemplateInfo
{
    public string FileName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public DateTime LastModifiedUtc { get; set; }
}
