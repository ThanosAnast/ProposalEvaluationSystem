using System.Globalization;
using System.Text;
using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public sealed class ExperimentCsvExporter : IExperimentCsvExporter
{
    public string ExportExperimentRuns(IReadOnlyCollection<ExperimentRun> runs)
    {
        var builder = new StringBuilder();
        builder.AppendLine(
            "dataset_id,run_id,created_at_utc,profile,programme,model," +
            "evaluation_configured_model,evaluation_reasoning_effort,evaluation_max_output_tokens," +
            "comparison_configured_model,comparison_reasoning_effort,comparison_max_output_tokens,comparison_response_model," +
            "prompt_file,prompt_sha256,input_fingerprint," +
            "llm_total,esr_total,total_difference,llm_threshold_met,esr_threshold_met,threshold_agreement," +
            "shared_strengths_count,shared_weaknesses_count,llm_only_count,esr_only_count," +
            "evaluation_response_id,comparison_response_id,evaluation_input_tokens,evaluation_output_tokens," +
            "evaluation_total_tokens,comparison_input_tokens,comparison_output_tokens,comparison_total_tokens," +
            "total_input_tokens,total_output_tokens,total_tokens,evaluation_duration_ms,comparison_duration_ms," +
            "total_duration_ms,git_commit_sha,experiment_schema_version");

        foreach (var run in runs.OrderBy(item => item.Metadata.CreatedAtUtc))
        {
            var comparison = run.ComparisonResult;
            var evaluationApi = run.Metadata.EvaluationApiMetadata;
            var comparisonApi = run.Metadata.ComparisonApiMetadata;
            var criteria = comparison?.Criteria ?? [];
            var evaluationDuration = evaluationApi?.DurationMilliseconds ?? run.Metadata.ExecutionDurationMilliseconds;
            var comparisonDuration = comparisonApi?.DurationMilliseconds ?? 0;

            AppendRow(builder,
                run.Metadata.DatasetId,
                run.Metadata.RunId,
                run.Metadata.CreatedAtUtc.ToString("O", CultureInfo.InvariantCulture),
                run.Metadata.EvaluationProfileId,
                run.Metadata.ProgrammeType.ToString(),
                run.Metadata.ModelName,
                run.Metadata.OpenAiRequestSnapshot?.ConfiguredModel,
                run.Metadata.OpenAiRequestSnapshot?.ReasoningEffort,
                Int(run.Metadata.OpenAiRequestSnapshot?.MaxOutputTokens),
                run.Metadata.ComparisonOpenAiRequestSnapshot?.ConfiguredModel,
                run.Metadata.ComparisonOpenAiRequestSnapshot?.ReasoningEffort,
                Int(run.Metadata.ComparisonOpenAiRequestSnapshot?.MaxOutputTokens),
                comparisonApi?.Model,
                run.Metadata.PromptFileName,
                run.Metadata.PromptContentSha256,
                run.Metadata.InputFingerprint,
                Format(run.IndependentEvaluation.TotalScore),
                Format(comparison?.OfficialTotalScore),
                Format(comparison?.TotalScoreDifference),
                Bool(run.IndependentEvaluation.ThresholdAssessment.Passed),
                Bool(comparison?.OfficialThresholdMet),
                Bool(comparison?.ThresholdAgreement),
                criteria.Sum(item => item.SharedStrengths.Count).ToString(CultureInfo.InvariantCulture),
                criteria.Sum(item => item.SharedWeaknesses.Count).ToString(CultureInfo.InvariantCulture),
                criteria.Sum(item => item.FindingsDetectedOnlyByLlm.Count).ToString(CultureInfo.InvariantCulture),
                criteria.Sum(item => item.FindingsPresentOnlyInEsr.Count).ToString(CultureInfo.InvariantCulture),
                evaluationApi?.ResponseId,
                comparisonApi?.ResponseId,
                Int(evaluationApi?.InputTokens),
                Int(evaluationApi?.OutputTokens),
                Int(evaluationApi?.TotalTokens),
                Int(comparisonApi?.InputTokens),
                Int(comparisonApi?.OutputTokens),
                Int(comparisonApi?.TotalTokens),
                (evaluationApi?.InputTokens + comparisonApi?.InputTokens ?? evaluationApi?.InputTokens ?? comparisonApi?.InputTokens)?.ToString(CultureInfo.InvariantCulture),
                (evaluationApi?.OutputTokens + comparisonApi?.OutputTokens ?? evaluationApi?.OutputTokens ?? comparisonApi?.OutputTokens)?.ToString(CultureInfo.InvariantCulture),
                (evaluationApi?.TotalTokens + comparisonApi?.TotalTokens ?? evaluationApi?.TotalTokens ?? comparisonApi?.TotalTokens)?.ToString(CultureInfo.InvariantCulture),
                evaluationDuration.ToString(CultureInfo.InvariantCulture),
                comparisonDuration.ToString(CultureInfo.InvariantCulture),
                (evaluationDuration + comparisonDuration).ToString(CultureInfo.InvariantCulture),
                run.Metadata.GitCommitSha,
                run.Metadata.ExperimentSchemaVersion);
        }

        return builder.ToString();
    }

    public string ExportCriterionComparisons(IReadOnlyCollection<ExperimentRun> runs)
    {
        var builder = new StringBuilder();
        builder.AppendLine(
            "dataset_id,run_id,created_at_utc,profile,model," +
            "evaluation_configured_model,evaluation_reasoning_effort,evaluation_max_output_tokens," +
            "comparison_configured_model,comparison_reasoning_effort,comparison_max_output_tokens,comparison_response_model," +
            "criterion_id,criterion_name,llm_score,esr_score," +
            "score_difference,llm_total,esr_total,total_difference,threshold_agreement,shared_strengths_count," +
            "shared_weaknesses_count,llm_only_count,esr_only_count,summary,evaluation_duration_ms," +
            "comparison_duration_ms,evaluation_input_tokens,evaluation_output_tokens,evaluation_total_tokens," +
            "comparison_input_tokens,comparison_output_tokens,comparison_total_tokens");

        foreach (var run in runs.OrderBy(item => item.Metadata.CreatedAtUtc))
        {
            var comparison = run.ComparisonResult;
            if (comparison is null)
            {
                continue;
            }

            var evaluationApi = run.Metadata.EvaluationApiMetadata;
            var comparisonApi = run.Metadata.ComparisonApiMetadata;
            var evaluationDuration = evaluationApi?.DurationMilliseconds ?? run.Metadata.ExecutionDurationMilliseconds;
            var comparisonDuration = comparisonApi?.DurationMilliseconds ?? 0;

            foreach (var criterion in comparison.Criteria)
            {
                AppendRow(builder,
                    run.Metadata.DatasetId,
                    run.Metadata.RunId,
                    run.Metadata.CreatedAtUtc.ToString("O", CultureInfo.InvariantCulture),
                    run.Metadata.EvaluationProfileId,
                    run.Metadata.ModelName,
                    run.Metadata.OpenAiRequestSnapshot?.ConfiguredModel,
                    run.Metadata.OpenAiRequestSnapshot?.ReasoningEffort,
                    Int(run.Metadata.OpenAiRequestSnapshot?.MaxOutputTokens),
                    run.Metadata.ComparisonOpenAiRequestSnapshot?.ConfiguredModel,
                    run.Metadata.ComparisonOpenAiRequestSnapshot?.ReasoningEffort,
                    Int(run.Metadata.ComparisonOpenAiRequestSnapshot?.MaxOutputTokens),
                    comparisonApi?.Model,
                    criterion.CriterionId,
                    criterion.CriterionName,
                    Format(criterion.LlmScore),
                    Format(criterion.OfficialScore),
                    Format(criterion.NumericDifference),
                    Format(comparison.LlmTotalScore),
                    Format(comparison.OfficialTotalScore),
                    Format(comparison.TotalScoreDifference),
                    Bool(comparison.ThresholdAgreement),
                    criterion.SharedStrengths.Count.ToString(CultureInfo.InvariantCulture),
                    criterion.SharedWeaknesses.Count.ToString(CultureInfo.InvariantCulture),
                    criterion.FindingsDetectedOnlyByLlm.Count.ToString(CultureInfo.InvariantCulture),
                    criterion.FindingsPresentOnlyInEsr.Count.ToString(CultureInfo.InvariantCulture),
                    criterion.Summary,
                    evaluationDuration.ToString(CultureInfo.InvariantCulture),
                    comparisonDuration.ToString(CultureInfo.InvariantCulture),
                    Int(evaluationApi?.InputTokens),
                    Int(evaluationApi?.OutputTokens),
                    Int(evaluationApi?.TotalTokens),
                    Int(comparisonApi?.InputTokens),
                    Int(comparisonApi?.OutputTokens),
                    Int(comparisonApi?.TotalTokens));
            }
        }

        return builder.ToString();
    }

    private static void AppendRow(StringBuilder builder, params string?[] values) =>
        builder.AppendLine(string.Join(',', values.Select(Escape)));

    private static string Format(decimal value) => value.ToString("0.############################", CultureInfo.InvariantCulture);

    private static string? Format(decimal? value) =>
        value?.ToString("0.############################", CultureInfo.InvariantCulture);

    private static string? Bool(bool? value) => value?.ToString().ToLowerInvariant();

    private static string? Int(int? value) => value?.ToString(CultureInfo.InvariantCulture);

    private static string Escape(string? value) =>
        $"\"{(value ?? string.Empty).Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
