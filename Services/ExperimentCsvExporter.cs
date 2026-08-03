using System.Globalization;
using System.Text;
using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public sealed class ExperimentCsvExporter : IExperimentCsvExporter
{
    public string Export(IReadOnlyCollection<ExperimentRunSummary> summaries)
    {
        var builder = new StringBuilder();
        builder.AppendLine("dataset_id,run_id,created_at_utc,profile,model,total_score,threshold_result,has_esr_comparison");

        foreach (var summary in summaries.OrderBy(item => item.CreatedAtUtc))
        {
            builder.Append(Escape(summary.DatasetId)).Append(',')
                .Append(Escape(summary.RunId)).Append(',')
                .Append(Escape(summary.CreatedAtUtc.ToString("O", CultureInfo.InvariantCulture))).Append(',')
                .Append(Escape(summary.EvaluationProfileId)).Append(',')
                .Append(Escape(summary.ModelName)).Append(',')
                .Append(summary.TotalScore.ToString("0.0", CultureInfo.InvariantCulture)).Append(',')
                .Append(Escape(summary.ThresholdResult)).Append(',')
                .AppendLine(summary.HasEsrComparison ? "true" : "false");
        }

        return builder.ToString();
    }

    private static string Escape(string value) =>
        $"\"{(value ?? string.Empty).Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
