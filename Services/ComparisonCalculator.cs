using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public sealed class ComparisonCalculator(IScoreCalculator scoreCalculator) : IComparisonCalculator
{
    public EvaluationComparisonResult Calculate(
        EvaluationProfile profile,
        EvaluationResult independentEvaluation,
        ReferenceEvaluation referenceEvaluation,
        ComparisonQualitativeDraft qualitativeDraft)
    {
        if (!string.Equals(independentEvaluation.EvaluationProfileId, profile.Id, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The independent evaluation does not match the selected profile.");
        }

        var officialCriteria = referenceEvaluation.Criteria.Select(reference => new CriterionEvaluation
        {
            CriterionId = reference.CriterionId,
            Name = profile.Criteria.FirstOrDefault(definition => definition.Id == reference.CriterionId)?.DisplayName
                ?? reference.CriterionId,
            Score = reference.OfficialScore
        }).ToArray();
        var officialCalculation = scoreCalculator.Calculate(profile, officialCriteria);
        var llmCalculation = scoreCalculator.Calculate(profile, independentEvaluation.Criteria);
        var officialById = officialCriteria.ToDictionary(criterion => criterion.CriterionId, StringComparer.Ordinal);
        var llmById = independentEvaluation.Criteria.ToDictionary(criterion => criterion.CriterionId, StringComparer.Ordinal);
        var qualitativeGroups = qualitativeDraft.Criteria
            .GroupBy(item => item.CriterionId, StringComparer.Ordinal)
            .ToArray();

        if (qualitativeGroups.Any(group => group.Count() != 1) ||
            qualitativeGroups.Length != profile.Criteria.Count ||
            profile.Criteria.Any(definition => qualitativeGroups.All(group => group.Key != definition.Id)))
        {
            throw new InvalidOperationException(
                "The qualitative comparison must contain exactly one entry for every evaluation criterion.");
        }

        var qualitativeById = qualitativeGroups.ToDictionary(
            group => group.Key,
            group => group.Single(),
            StringComparer.Ordinal);

        var comparisons = profile.Criteria.Select(definition =>
        {
            var qualitative = qualitativeById[definition.Id];
            return new CriterionComparison
            {
                CriterionId = definition.Id,
                CriterionName = definition.DisplayName,
                OfficialScore = officialById[definition.Id].Score,
                LlmScore = llmById[definition.Id].Score,
                NumericDifference = llmById[definition.Id].Score - officialById[definition.Id].Score,
                SharedStrengths = qualitative.SharedStrengths,
                SharedWeaknesses = qualitative.SharedWeaknesses,
                FindingsDetectedOnlyByLlm = qualitative.FindingsDetectedOnlyByLlm,
                FindingsPresentOnlyInEsr = qualitative.FindingsPresentOnlyInEsr,
                Summary = qualitative.Summary
            };
        }).ToList();

        return new EvaluationComparisonResult
        {
            Criteria = comparisons,
            OfficialTotalScore = officialCalculation.TotalScore,
            LlmTotalScore = llmCalculation.TotalScore,
            TotalScoreDifference = llmCalculation.TotalScore - officialCalculation.TotalScore,
            OfficialThresholdMet = officialCalculation.ThresholdAssessment.Passed,
            LlmThresholdMet = llmCalculation.ThresholdAssessment.Passed,
            ThresholdAgreement = officialCalculation.ThresholdAssessment.Passed == llmCalculation.ThresholdAssessment.Passed,
            OverallComparisonSummary = qualitativeDraft.OverallComparisonSummary,
            ComparisonLimitations = qualitativeDraft.ComparisonLimitations
        };
    }
}
