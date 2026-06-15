using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public interface IEvaluationRunner
{
    Task<EvaluationResult> EvaluateAsync(EvaluationEngine engine, EvaluationRequest request, CancellationToken cancellationToken = default);
}
