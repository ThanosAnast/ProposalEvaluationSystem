namespace ProposalEvaluationSystem.Models;

public class EvaluationRequest
{
    public ProcessedDocument CallDocument { get; set; } = new() { DocumentType = DocumentType.CallDescription };

    public ProcessedDocument ProposalDocument { get; set; } = new() { DocumentType = DocumentType.Proposal };

    public ProcessedDocument? RealEvaluationDocument { get; set; }

    public ProgrammeType ProgrammeType { get; set; }

    public EvaluationLevel EvaluationLevel { get; set; }

    public string PromptTemplateName { get; set; } = string.Empty;

    public string PromptTemplateContent { get; set; } = string.Empty;

    public string GeneratedPrompt { get; set; } = string.Empty;
}
