using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Options;
using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public sealed class ExperimentRunFactory : IExperimentRunFactory
{
    private readonly ExperimentsOptions experimentOptions;
    private readonly string gitCommitSha;

    public ExperimentRunFactory(
        IOptions<ExperimentsOptions> options,
        IWebHostEnvironment environment)
    {
        experimentOptions = options.Value;
        gitCommitSha = ResolveGitCommitSha(experimentOptions.GitCommitSha, environment.ContentRootPath);
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
        ContentSha256 = ContentHashService.ComputeSha256(document.ExtractedText)
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
