using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public sealed class ExperimentJsonExporter : IExperimentJsonExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string Export(ExperimentRun run)
    {
        ArgumentNullException.ThrowIfNull(run);

        var root = JsonSerializer.SerializeToNode(run, JsonOptions)?.AsObject()
            ?? throw new InvalidOperationException("The experiment run could not be serialized.");

        // Downloads are deliberately safe to share for analysis: source document text
        // and the generated prompt are never included, even if local sensitive storage
        // was explicitly enabled for the repository.
        root.Remove("sensitiveContent");

        return root.ToJsonString(JsonOptions);
    }
}
