using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public sealed class EvaluationRunner(IEvaluationService evaluationService) : IEvaluationRunner, IDisposable
{
    private readonly SemaphoreSlim requestGate = new(1, 1);

    public async Task<EvaluationResult> EvaluateAsync(
        EvaluationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await requestGate.WaitAsync(0, cancellationToken))
        {
            throw new InvalidOperationException("An independent evaluation is already running.");
        }

        try
        {
            return await evaluationService.EvaluateAsync(request, cancellationToken);
        }
        finally
        {
            requestGate.Release();
        }
    }

    public void Dispose() => requestGate.Dispose();
}
