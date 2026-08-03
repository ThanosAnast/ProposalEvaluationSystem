using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public interface IEvaluationRunner
{
    Task<EvaluationResult> EvaluateAsync(EvaluationRequest request, CancellationToken cancellationToken = default);
}
