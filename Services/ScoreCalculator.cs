using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public sealed class ScoreCalculator : IScoreCalculator
{
    public ScoreCalculation Calculate(EvaluationProfile profile, IReadOnlyCollection<CriterionEvaluation> criteria)
    {
        EnsureConfigured(profile);

        var byId = criteria
            .GroupBy(criterion => criterion.Id, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

        var expectedIds = profile.Criteria.Select(definition => definition.Id).ToHashSet(StringComparer.Ordinal);
        if (criteria.Count != profile.Criteria.Count ||
            byId.Any(pair => pair.Value.Length != 1) ||
            !byId.Keys.ToHashSet(StringComparer.Ordinal).SetEquals(expectedIds))
        {
            throw new ScoreValidationException("The evaluation must contain exactly one score for every profile criterion.");
        }

        foreach (var criterion in criteria)
        {
            ValidateScore(profile, criterion.Score, criterion.Name);
        }

        var total = criteria.Sum(criterion => criterion.Score);
        var individualThresholdsMet = profile.Criteria.All(
            definition => byId[definition.Id][0].Score >= definition.Threshold);
        var overallThresholdMet = total >= profile.OverallThreshold!.Value;
        var passed = individualThresholdsMet && overallThresholdMet;

        return new ScoreCalculation
        {
            TotalScore = total,
            ThresholdAssessment = new ThresholdAssessment
            {
                TotalScore = total,
                RequiredTotalScore = profile.OverallThreshold.Value,
                MaximumTotalScore = profile.MaximumTotal,
                IndividualThresholdsMet = individualThresholdsMet,
                OverallThresholdMet = overallThresholdMet,
                Passed = passed,
                OverallResult = passed ? "Threshold met" : "Below threshold",
                Explanation = CreateExplanation(profile, total, individualThresholdsMet, overallThresholdMet)
            }
        };
    }

    public bool IsValidScore(EvaluationProfile profile, decimal score)
    {
        if (!profile.IsConfigured)
        {
            return false;
        }

        var minimum = profile.ScoreMinimum!.Value;
        var maximum = profile.ScoreMaximum!.Value;
        var increment = profile.ScoreIncrement!.Value;
        return score >= minimum && score <= maximum && decimal.Remainder(score - minimum, increment) == 0m;
    }

    private void ValidateScore(EvaluationProfile profile, decimal score, string criterionName)
    {
        if (!IsValidScore(profile, score))
        {
            throw new ScoreValidationException(
                $"Score {score} for '{criterionName}' is invalid. Scores must be between {profile.ScoreMinimum:0.0} and {profile.ScoreMaximum:0.0} in increments of {profile.ScoreIncrement:0.0}.");
        }
    }

    private static void EnsureConfigured(EvaluationProfile profile)
    {
        if (!profile.Enabled || !profile.IsConfigured)
        {
            throw new ScoreValidationException($"Evaluation profile '{profile.Id}' is not enabled or fully configured.");
        }

        if (profile.ScoreIncrement <= 0m || profile.ScoreMaximum < profile.ScoreMinimum)
        {
            throw new ScoreValidationException($"Evaluation profile '{profile.Id}' has an invalid score range.");
        }
    }

    private static string CreateExplanation(
        EvaluationProfile profile,
        decimal total,
        bool individualThresholdsMet,
        bool overallThresholdMet)
    {
        var individualText = individualThresholdsMet
            ? "All individual criterion thresholds are met."
            : "At least one individual criterion threshold is not met.";
        var overallText = overallThresholdMet
            ? $"The total score of {total:0.0} meets the required {profile.OverallThreshold:0.0}."
            : $"The total score of {total:0.0} is below the required {profile.OverallThreshold:0.0}.";

        return $"{individualText} {overallText}";
    }
}
