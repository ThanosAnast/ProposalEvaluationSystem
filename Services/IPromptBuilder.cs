using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public interface IPromptBuilder
{
    string Build(EvaluationRequest request);
}
