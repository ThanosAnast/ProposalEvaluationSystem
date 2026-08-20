namespace ProposalEvaluationSystem.Models;

public sealed class EvaluationRequest
{
    public ProcessedDocument CallDocument { get; set; } = new() { DocumentType = DocumentType.CallDescription };

    public ProcessedDocument ProposalDocument { get; set; } = new() { DocumentType = DocumentType.Proposal };


    public EvaluationProfile Profile { get; set; } = EvaluationProfiles.HorizonEuropeRiaIa;

    public string PromptTemplateName { get; set; } = string.Empty;

    public string PromptTemplateContent { get; set; } = string.Empty;

    public string GeneratedPrompt { get; set; } = string.Empty;

    public string InputFingerprint { get; set; } = string.Empty;
}
