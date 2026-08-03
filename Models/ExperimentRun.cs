namespace ProposalEvaluationSystem.Models;

public sealed class ExperimentRun
{
    public ExperimentRunMetadata Metadata { get; set; } = new();

    public EvaluationResult IndependentEvaluation { get; set; } = new();

    public EvaluationComparisonResult? ComparisonResult { get; set; }

    public List<string> Warnings { get; set; } = [];

    public List<string> Errors { get; set; } = [];

    public ExperimentSensitiveContent? SensitiveContent { get; set; }
}
