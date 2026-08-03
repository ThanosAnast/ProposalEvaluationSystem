using System.Reflection;
using Microsoft.Extensions.Options;
using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public sealed class ExperimentRunFactory(IOptions<ExperimentsOptions> options) : IExperimentRunFactory
{
    private readonly ExperimentsOptions experimentOptions = options.Value;

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
                    ?? "unknown"
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
}
