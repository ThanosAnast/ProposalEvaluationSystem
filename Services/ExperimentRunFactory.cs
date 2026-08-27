using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Options;
using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public sealed class ExperimentRunFactory : IExperimentRunFactory
{
    private readonly ExperimentsOptions experimentOptions;
    private readonly OpenAiOptions openAiOptions;
    private readonly string gitCommitSha;

    public ExperimentRunFactory(
        IOptions<ExperimentsOptions> experimentOptions,
        IOptions<OpenAiOptions> openAiOptions,
        IWebHostEnvironment environment)
    {
        this.experimentOptions = experimentOptions.Value;
        this.openAiOptions = openAiOptions.Value;
        gitCommitSha = ResolveGitCommitSha(this.experimentOptions.GitCommitSha, environment.ContentRootPath);
    }

    public ExperimentRun Create(ExperimentRunCreationRequest request)
    {
        var datasetId = FileExperimentRepository.ValidateDatasetId(request.DatasetId);
        var esrProvided = request.EsrDocument is not null &&
            !string.IsNullOrWhiteSpace(request.EsrDocument.ExtractedText);

        return new ExperimentRun
        {
            Metadata = new ExperimentRunMetadata
            {
                RunId = Guid.NewGuid().ToString("N"),
                DatasetId = datasetId,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                EvaluationProfileId = request.Profile.Id,
                ProgrammeType = request.Profile.ProgrammeType,
                ModelName = request.IndependentEvaluation.ModelName,
                PromptFileName = request.IndependentEvaluation.PromptTemplateUsed,
                PromptContentSha256 = ContentHashService.ComputeSha256(request.PromptContent),
                Call = CreateDocumentMetadata(request.CallDocument),
                Proposal = CreateDocumentMetadata(request.ProposalDocument),
                Esr = esrProvided ? CreateDocumentMetadata(request.EsrDocument!) : null,
                InputFingerprint = request.IndependentEvaluation.InputFingerprint,
                ExecutionDurationMilliseconds = (long)request.ExecutionDuration.TotalMilliseconds,
                ApplicationVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
                    ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                    ?? "unknown",
                GitCommitSha = gitCommitSha,
                ExperimentSchemaVersion = string.IsNullOrWhiteSpace(experimentOptions.ExperimentSchemaVersion)
                    ? ExperimentsOptions.DefaultSchemaVersion
                    : experimentOptions.ExperimentSchemaVersion,
                ProfileSnapshot = CreateProfileSnapshot(request.Profile),
                PromptTemplateSnapshot = new PromptTemplateSnapshot
                {
                    FileName = request.IndependentEvaluation.PromptTemplateUsed,
                    ContentSha256 = ContentHashService.ComputeSha256(request.PromptContent),
                    Content = request.PromptContent
                },
                OpenAiRequestSnapshot = new OpenAiRequestSnapshot
                {
                    ConfiguredModel = openAiOptions.Model,
                    ReasoningEffort = openAiOptions.ReasoningEffort,
                    MaxOutputTokens = openAiOptions.MaxOutputTokens,
                    TimeoutSeconds = openAiOptions.TimeoutSeconds,
                    MaxAttempts = openAiOptions.MaxAttempts,
                    InitialRetryDelayMilliseconds = openAiOptions.InitialRetryDelayMilliseconds,
                    StoreResponse = false,
                    Endpoint = openAiOptions.Endpoint
                },
                ComparisonOpenAiRequestSnapshot = request.ComparisonResult is null
                    ? null
                    : new OpenAiRequestSnapshot
                    {
                        ConfiguredModel = openAiOptions.ComparisonModel,
                        ReasoningEffort = openAiOptions.ComparisonReasoningEffort,
                        MaxOutputTokens = openAiOptions.ComparisonMaxOutputTokens,
                        TimeoutSeconds = openAiOptions.TimeoutSeconds,
                        MaxAttempts = openAiOptions.MaxAttempts,
                        InitialRetryDelayMilliseconds = openAiOptions.InitialRetryDelayMilliseconds,
                        StoreResponse = false,
                        Endpoint = openAiOptions.Endpoint
                    },
                EvaluationApiMetadata = request.IndependentEvaluation.ApiMetadata,
                ComparisonApiMetadata = request.ComparisonResult?.ApiMetadata
            },
            IndependentEvaluation = request.IndependentEvaluation,
            ComparisonResult = request.ComparisonResult,
            Warnings = request.Warnings.ToList(),
            Errors = request.Errors.ToList(),
            SensitiveContent = experimentOptions.StoreSensitiveContent
                ? new ExperimentSensitiveContent
                {
                    CallText = request.CallDocument.ExtractedText,
                    ProposalText = request.ProposalDocument.ExtractedText,
                    EsrText = esrProvided ? request.EsrDocument!.ExtractedText : null,
                    GeneratedPrompt = request.GeneratedPrompt
                }
                : null
        };
    }

    private static ExperimentDocumentMetadata CreateDocumentMetadata(ProcessedDocument document) => new()
    {
        FileName = document.FileName,
        ContentSha256 = ContentHashService.ComputeSha256(document.ExtractedText),
        DocumentType = document.DocumentType,
        ExtractedCharacterCount = document.CharacterCount,
        ExtractionSucceeded = document.ExtractionSucceeded
    };

    private static EvaluationProfileSnapshot CreateProfileSnapshot(EvaluationProfile profile) => new()
    {
        Id = profile.Id,
        ProgrammeType = profile.ProgrammeType,
        DisplayName = profile.DisplayName,
        PromptTemplateFileName = profile.PromptTemplateFileName,
        ScoringMode = profile.ScoringMode,
        OverallThreshold = profile.OverallThreshold,
        MaximumTotal = profile.MaximumTotal,
        Criteria = profile.Criteria.Select(criterion => new EvaluationCriterionSnapshot
        {
            Id = criterion.Id,
            DisplayName = criterion.DisplayName,
            ScoreMinimum = criterion.ScoreMinimum,
            ScoreMaximum = criterion.ScoreMaximum,
            ScoreIncrement = criterion.ScoreIncrement,
            Threshold = criterion.Threshold,
            WeightPercentage = criterion.WeightPercentage
        }).ToList()
    };

    private static string ResolveGitCommitSha(string? configuredSha, string contentRootPath)
    {
        if (!string.IsNullOrWhiteSpace(configuredSha))
        {
            return configuredSha.Trim();
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                WorkingDirectory = contentRootPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            startInfo.ArgumentList.Add("rev-parse");
            startInfo.ArgumentList.Add("HEAD");

            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return "unknown";
            }

            var output = process.StandardOutput.ReadToEnd().Trim();
            return process.WaitForExit(2000) && process.ExitCode == 0 && output.All(Uri.IsHexDigit)
                ? output
                : "unknown";
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            return "unknown";
        }
    }
}
