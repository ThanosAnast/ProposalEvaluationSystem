using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

/// <summary>
/// Explicit test/development double. It is intentionally not registered in the application container.
/// </summary>
public sealed class MockEvaluationService(
    Func<EvaluationRequest, CancellationToken, Task<EvaluationResult>> implementation) : IEvaluationService
{
    public Task<EvaluationResult> EvaluateAsync(
        EvaluationRequest request,
        CancellationToken cancellationToken = default) => implementation(request, cancellationToken);
}
