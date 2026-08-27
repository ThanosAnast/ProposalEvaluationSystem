using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public interface IScoreCalculator
{
    ScoreCalculation Calculate(EvaluationProfile profile, IReadOnlyCollection<CriterionEvaluation> criteria);

    ScoreCalculation CalculateReference(
        EvaluationProfile profile,
        IReadOnlyCollection<CriterionEvaluation> criteria);

    bool IsValidScore(EvaluationProfile profile, string criterionId, decimal score);

    bool IsValidReferenceScore(EvaluationProfile profile, string criterionId, decimal score);
}
