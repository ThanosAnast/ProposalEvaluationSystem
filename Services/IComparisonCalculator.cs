using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public interface IComparisonCalculator
{
    EvaluationComparisonResult Calculate(
        EvaluationProfile profile,
        EvaluationResult independentEvaluation,
        ReferenceEvaluation referenceEvaluation,
        ComparisonQualitativeDraft qualitativeDraft);
}
