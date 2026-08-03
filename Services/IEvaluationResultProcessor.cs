using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public interface IEvaluationResultProcessor
{
    EvaluationResult Process(EvaluationDraft draft, EvaluationRequest request, string modelName);
}
