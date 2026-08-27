using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public sealed class ScoreCalculator : IScoreCalculator
{
    public ScoreCalculation Calculate(EvaluationProfile profile, IReadOnlyCollection<CriterionEvaluation> criteria)
        => CalculateCore(profile, criteria, enforceScoreIncrement: true);

    public ScoreCalculation CalculateReference(
        EvaluationProfile profile,
        IReadOnlyCollection<CriterionEvaluation> criteria)
        => CalculateCore(profile, criteria, enforceScoreIncrement: false);

    private ScoreCalculation CalculateCore(
        EvaluationProfile profile,
        IReadOnlyCollection<CriterionEvaluation> criteria,
        bool enforceScoreIncrement)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(criteria);
        EnsureConfigured(profile);

        if (criteria.Any(criterion => string.IsNullOrWhiteSpace(criterion.CriterionId)))
        {
            throw new ScoreValidationException("Every evaluation criterion must have a criterion ID.");
        }

        var byId = criteria
            .GroupBy(criterion => criterion.CriterionId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

        var expectedIds = profile.Criteria.Select(definition => definition.Id).ToHashSet(StringComparer.Ordinal);
        if (criteria.Count != profile.Criteria.Count ||
            byId.Any(pair => pair.Value.Length != 1) ||
            !byId.Keys.ToHashSet(StringComparer.Ordinal).SetEquals(expectedIds))
        {
            throw new ScoreValidationException("The evaluation must contain exactly one score for every profile criterion.");
        }

        foreach (var definition in profile.Criteria)
        {
            ValidateScore(profile, definition, byId[definition.Id][0].Score, enforceScoreIncrement);
        }

        var criterionCalculations = profile.Criteria
            .Select(definition => CreateCriterionCalculation(
                profile.ScoringMode,
                definition,
                byId[definition.Id][0].Score))
            .ToArray();
        var total = criterionCalculations.Sum(item => item.ContributionToTotal);

        var applicableThresholds = profile.Criteria
            .Where(definition => definition.Threshold.HasValue)
            .ToArray();
        var individualThresholdsMet = applicableThresholds.All(
            definition => byId[definition.Id][0].Score >= definition.Threshold!.Value);
        var overallThresholdMet = total >= profile.OverallThreshold;
        var passed = individualThresholdsMet && overallThresholdMet;

        return new ScoreCalculation
        {
            TotalScore = total,
            Criteria = criterionCalculations,
            ThresholdAssessment = new ThresholdAssessment
            {
                TotalScore = total,
                RequiredTotalScore = profile.OverallThreshold,
                MaximumTotalScore = profile.MaximumTotal,
                IndividualThresholdsMet = individualThresholdsMet,
                OverallThresholdMet = overallThresholdMet,
                Passed = passed,
                OverallResult = passed ? "Threshold met" : "Below threshold",
                Explanation = CreateExplanation(
                    profile,
                    total,
                    applicableThresholds.Length,
                    individualThresholdsMet,
                    overallThresholdMet)
            }
        };
    }

    private static CriterionScoreCalculation CreateCriterionCalculation(
        ScoringMode scoringMode,
        CriterionDefinition definition,
        decimal rawScore)
    {
        var contribution = scoringMode switch
        {
            ScoringMode.Additive => rawScore,
            ScoringMode.WeightedPercentage =>
                rawScore / definition.ScoreMaximum * definition.WeightPercentage!.Value,
            _ => throw new ScoreValidationException("The evaluation profile uses an unsupported scoring mode.")
        };

        return new CriterionScoreCalculation
        {
            CriterionId = definition.Id,
            CriterionName = definition.DisplayName,
            RawScore = rawScore,
            ScoreMinimum = definition.ScoreMinimum,
            ScoreMaximum = definition.ScoreMaximum,
            ScoreIncrement = definition.ScoreIncrement,
            WeightPercentage = definition.WeightPercentage,
            ContributionToTotal = contribution,
            Threshold = definition.Threshold,
            ThresholdMet = definition.Threshold.HasValue
                ? rawScore >= definition.Threshold.Value
                : null
        };
    }

    public bool IsValidScore(EvaluationProfile profile, string criterionId, decimal score)
        => IsValidScoreCore(profile, criterionId, score, enforceScoreIncrement: true);

    public bool IsValidReferenceScore(EvaluationProfile profile, string criterionId, decimal score)
        => IsValidScoreCore(profile, criterionId, score, enforceScoreIncrement: false);

    private static bool IsValidScoreCore(
        EvaluationProfile profile,
        string criterionId,
        decimal score,
        bool enforceScoreIncrement)
    {
        if (profile is null ||
            !profile.Enabled ||
            !profile.IsConfigured ||
            string.IsNullOrWhiteSpace(criterionId))
        {
            return false;
        }

        var definition = profile.Criteria.FirstOrDefault(
            candidate => string.Equals(candidate.Id, criterionId, StringComparison.Ordinal));
        if (definition is null || score < definition.ScoreMinimum || score > definition.ScoreMaximum)
        {
            return false;
        }

        return !enforceScoreIncrement ||
               !definition.ScoreIncrement.HasValue ||
               decimal.Remainder(score - definition.ScoreMinimum, definition.ScoreIncrement.Value) == 0m;
    }

    private void ValidateScore(
        EvaluationProfile profile,
        CriterionDefinition definition,
        decimal score,
        bool enforceScoreIncrement)
    {
        if (IsValidScoreCore(profile, definition.Id, score, enforceScoreIncrement))
        {
            return;
        }

        var incrementText = enforceScoreIncrement && definition.ScoreIncrement.HasValue
            ? $" in increments of {definition.ScoreIncrement:0.##}"
            : string.Empty;
        throw new ScoreValidationException(
            $"Score {score} for '{definition.DisplayName}' is invalid. Scores must be between " +
            $"{definition.ScoreMinimum:0.##} and {definition.ScoreMaximum:0.##}{incrementText}.");
    }

    private static void EnsureConfigured(EvaluationProfile profile)
    {
        if (!profile.Enabled || !profile.IsConfigured)
        {
            throw new ScoreValidationException($"Evaluation profile '{profile.Id}' is not enabled or fully configured.");
        }
    }

    private static string CreateExplanation(
        EvaluationProfile profile,
        decimal total,
        int applicableThresholdCount,
        bool individualThresholdsMet,
        bool overallThresholdMet)
    {
        var individualText = applicableThresholdCount == 0
            ? "No individual criterion thresholds apply."
            : individualThresholdsMet
                ? "All applicable individual criterion thresholds are met."
                : "At least one applicable individual criterion threshold is not met.";
        var totalDescription = profile.ScoringMode == ScoringMode.WeightedPercentage
            ? "weighted total score"
            : "total score";
        var overallText = overallThresholdMet
            ? $"The {totalDescription} of {total:0.##} meets the required " +
              $"{profile.OverallThreshold:0.##} out of {profile.MaximumTotal:0.##}."
            : $"The {totalDescription} of {total:0.##} is below the required " +
              $"{profile.OverallThreshold:0.##} out of {profile.MaximumTotal:0.##}.";

        return $"{individualText} {overallText}";
    }
}
