using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public interface IExperimentCsvExporter
{
    string Export(IReadOnlyCollection<ExperimentRunSummary> summaries);
}
