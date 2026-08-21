using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public interface IExperimentJsonExporter
{
    string Export(ExperimentRun run);
}
