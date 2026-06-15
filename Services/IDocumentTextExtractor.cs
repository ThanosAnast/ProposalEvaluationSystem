using Microsoft.AspNetCore.Components.Forms;
using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public interface IDocumentTextExtractor
{
    Task<ProcessedDocument> ExtractAsync(IBrowserFile file, DocumentType documentType, CancellationToken cancellationToken = default);
}
