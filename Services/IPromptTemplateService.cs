using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public interface IPromptTemplateService
{
    Task<IReadOnlyList<PromptTemplateInfo>> GetTemplatesAsync(CancellationToken cancellationToken = default);

    Task<string> GetTemplateContentAsync(string fileName, CancellationToken cancellationToken = default);

    Task SaveTemplateAsync(string fileName, string content, CancellationToken cancellationToken = default);

    Task<PromptTemplateInfo> CreateTemplateAsync(string fileName, string content, CancellationToken cancellationToken = default);

    Task<PromptTemplateInfo> DuplicateTemplateAsync(string sourceFileName, string? newFileName = null, CancellationToken cancellationToken = default);
}
