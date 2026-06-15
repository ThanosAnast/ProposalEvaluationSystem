using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public class EvaluationRunner(
    MockEvaluationService mockEvaluationService,
    OpenAiEvaluationService openAiEvaluationService) : IEvaluationRunner
{
    public Task<EvaluationResult> EvaluateAsync(EvaluationEngine engine, EvaluationRequest request, CancellationToken cancellationToken = default) =>
        engine switch
        {
            EvaluationEngine.Mock => mockEvaluationService.EvaluateAsync(request, cancellationToken),
            EvaluationEngine.OpenAI => openAiEvaluationService.EvaluateAsync(request, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(engine), engine, "Unsupported evaluation engine.")
        };
}
