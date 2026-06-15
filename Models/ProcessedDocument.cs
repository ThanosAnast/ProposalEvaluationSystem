namespace ProposalEvaluationSystem.Models;

public class ProcessedDocument
{
    public string FileName { get; set; } = string.Empty;

    public DocumentType DocumentType { get; set; }

    public string ExtractedText { get; set; } = string.Empty;

    public int CharacterCount => ExtractedText.Length;

    public bool ExtractionSucceeded { get; set; }

    public List<string> Warnings { get; set; } = [];
}
