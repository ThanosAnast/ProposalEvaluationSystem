using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public interface IExperimentRunFactory
{
    ExperimentRun Create(ExperimentRunCreationRequest request);
}
