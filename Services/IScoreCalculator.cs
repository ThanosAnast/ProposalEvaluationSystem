using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public interface IScoreCalculator
{
    ScoreCalculation Calculate(EvaluationProfile profile, IReadOnlyCollection<CriterionEvaluation> criteria);

    bool IsValidScore(EvaluationProfile profile, string criterionId, decimal score);
}
