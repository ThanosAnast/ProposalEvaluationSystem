using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public interface IEvaluationService
{
    Task<EvaluationResult> EvaluateAsync(EvaluationRequest request, CancellationToken cancellationToken = default);
}
