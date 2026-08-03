using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public sealed class EvaluationWorkflowState
{
    public string? GeneratedPrompt { get; set; }

    public EvaluationResult? IndependentEvaluation { get; set; }

    public EvaluationComparisonResult? EsrComparison { get; set; }

    public string? ExperimentRunId { get; set; }

    public void InvalidateIndependentInputs()
    {
        GeneratedPrompt = null;
        IndependentEvaluation = null;
        EsrComparison = null;
        ExperimentRunId = null;
    }

    public void InvalidateReferenceInputs()
    {
        EsrComparison = null;
    }
}
