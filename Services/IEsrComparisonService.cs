using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public interface IEsrComparisonService
{
    Task<EvaluationComparisonResult> CompareAsync(
        EsrComparisonRequest request,
        CancellationToken cancellationToken = default);
}
