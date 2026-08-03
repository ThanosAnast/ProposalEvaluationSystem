using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public interface IExperimentRepository
{
    Task SaveAsync(ExperimentRun run, CancellationToken cancellationToken = default);

    Task<ExperimentRun?> GetAsync(string datasetId, string runId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExperimentRun>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExperimentRunSummary>> GetSummariesAsync(CancellationToken cancellationToken = default);
}
