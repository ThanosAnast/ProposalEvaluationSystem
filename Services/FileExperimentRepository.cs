using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public sealed partial class FileExperimentRepository : IExperimentRepository, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string experimentRoot;
    private readonly ExperimentsOptions options;
    private readonly SemaphoreSlim fileGate = new(1, 1);

    public FileExperimentRepository(IWebHostEnvironment environment, IOptions<ExperimentsOptions> options)
        : this(Path.Combine(environment.ContentRootPath, "App_Data", "ExperimentRuns"), options)
    {
    }

    public FileExperimentRepository(string experimentRoot, IOptions<ExperimentsOptions> options)
    {
        this.experimentRoot = Path.GetFullPath(experimentRoot);
        this.options = options.Value;
    }

    public async Task SaveAsync(ExperimentRun run, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(run);
        var datasetId = ValidateDatasetId(run.Metadata.DatasetId);
        var runId = ValidateRunId(run.Metadata.RunId);
        var datasetDirectory = Path.Combine(experimentRoot, datasetId);
        var timestamp = run.Metadata.CreatedAtUtc.UtcDateTime.ToString("yyyyMMddTHHmmssfffZ");
        var destinationPath = Path.Combine(datasetDirectory, $"{timestamp}_{runId}.json");
        var temporaryPath = destinationPath + ".tmp";

        await fileGate.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(datasetDirectory);
            var jsonNode = JsonSerializer.SerializeToNode(run, JsonOptions)?.AsObject()
                ?? throw new InvalidOperationException("The experiment run could not be serialized.");

            if (!options.StoreSensitiveContent)
            {
                jsonNode.Remove("sensitiveContent");
            }

            await File.WriteAllTextAsync(temporaryPath, jsonNode.ToJsonString(JsonOptions), cancellationToken);
            File.Move(temporaryPath, destinationPath, overwrite: false);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }

            fileGate.Release();
        }
    }

    public async Task<ExperimentRun?> GetAsync(
        string datasetId,
        string runId,
        CancellationToken cancellationToken = default)
    {
        datasetId = ValidateDatasetId(datasetId);
        runId = ValidateRunId(runId);

        await fileGate.WaitAsync(cancellationToken);
        try
        {
            var datasetDirectory = Path.Combine(experimentRoot, datasetId);
            if (!Directory.Exists(datasetDirectory))
            {
                return null;
            }

            var path = Directory.EnumerateFiles(datasetDirectory, $"*_{runId}.json").SingleOrDefault();
            if (path is null)
            {
                return null;
            }

            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<ExperimentRun>(stream, JsonOptions, cancellationToken);
        }
        finally
        {
            fileGate.Release();
        }
    }

    public async Task<IReadOnlyList<ExperimentRunSummary>> GetSummariesAsync(
        CancellationToken cancellationToken = default)
    {
        await fileGate.WaitAsync(cancellationToken);
        try
        {
            var runs = await ReadAllWithoutLockAsync(cancellationToken);
            return runs
                .Select(run => new ExperimentRunSummary
                {
                    DatasetId = run.Metadata.DatasetId,
                    RunId = run.Metadata.RunId,
                    CreatedAtUtc = run.Metadata.CreatedAtUtc,
                    EvaluationProfileId = run.Metadata.EvaluationProfileId,
                    ModelName = run.Metadata.ModelName,
                    TotalScore = run.IndependentEvaluation.TotalScore,
                    ThresholdResult = run.IndependentEvaluation.ThresholdAssessment.OverallResult,
                    HasEsrComparison = run.ComparisonResult is not null
                })
                .OrderByDescending(summary => summary.CreatedAtUtc)
                .ToArray();
        }
        finally
        {
            fileGate.Release();
        }
    }

    public async Task<IReadOnlyList<ExperimentRun>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await fileGate.WaitAsync(cancellationToken);
        try
        {
            var runs = await ReadAllWithoutLockAsync(cancellationToken);
            return runs
                .OrderBy(run => run.Metadata.CreatedAtUtc)
                .ToArray();
        }
        finally
        {
            fileGate.Release();
        }
    }

    private async Task<List<ExperimentRun>> ReadAllWithoutLockAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(experimentRoot))
        {
            return [];
        }

        var runs = new List<ExperimentRun>();
        foreach (var path in Directory.EnumerateFiles(experimentRoot, "*.json", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await using var stream = File.OpenRead(path);
            var run = await JsonSerializer.DeserializeAsync<ExperimentRun>(stream, JsonOptions, cancellationToken);
            if (run is not null)
            {
                runs.Add(run);
            }
        }

        return runs;
    }

    public static string ValidateDatasetId(string datasetId)
    {
        var value = datasetId?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Dataset ID is required.", nameof(datasetId));
        }

        if (value.Length > 100 || !DatasetIdPattern().IsMatch(value))
        {
            throw new ArgumentException(
                "Dataset ID may contain only letters, numbers, periods, underscores, and hyphens.",
                nameof(datasetId));
        }

        return value;
    }

    private static string ValidateRunId(string runId)
    {
        var value = runId?.Trim() ?? string.Empty;
        if (value.Length != 32 || !value.All(Uri.IsHexDigit))
        {
            throw new ArgumentException("Run ID is invalid.", nameof(runId));
        }

        return value.ToLowerInvariant();
    }

    public void Dispose() => fileGate.Dispose();

    [GeneratedRegex("^[A-Za-z0-9._-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex DatasetIdPattern();
}
