using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public interface IExperimentCsvExporter
{
    string ExportExperimentRuns(IReadOnlyCollection<ExperimentRun> runs);

    string ExportCriterionComparisons(IReadOnlyCollection<ExperimentRun> runs);
}
