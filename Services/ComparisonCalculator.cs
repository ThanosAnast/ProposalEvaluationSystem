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
            Id = reference.CriterionId,
            Name = profile.Criteria.FirstOrDefault(definition => definition.Id == reference.CriterionId)?.DisplayName
                ?? reference.CriterionId,
            Score = reference.OfficialScore
        }).ToArray();
        var officialCalculation = scoreCalculator.Calculate(profile, officialCriteria);
        var llmCalculation = scoreCalculator.Calculate(profile, independentEvaluation.Criteria);
        var officialById = officialCriteria.ToDictionary(criterion => criterion.Id, StringComparer.Ordinal);
        var llmById = independentEvaluation.Criteria.ToDictionary(criterion => criterion.Id, StringComparer.Ordinal);

        var comparisons = profile.Criteria.Select(definition => new CriterionComparison
        {
            CriterionId = definition.Id,
            CriterionName = definition.DisplayName,
            OfficialScore = officialById[definition.Id].Score,
            LlmScore = llmById[definition.Id].Score,
            NumericDifference = llmById[definition.Id].Score - officialById[definition.Id].Score
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
            SharedStrengths = qualitativeDraft.SharedStrengths,
            SharedWeaknesses = qualitativeDraft.SharedWeaknesses,
            FindingsDetectedOnlyByLlm = qualitativeDraft.FindingsDetectedOnlyByLlm,
            FindingsPresentOnlyInEsr = qualitativeDraft.FindingsPresentOnlyInEsr,
            OverallComparisonSummary = qualitativeDraft.OverallComparisonSummary,
            ComparisonLimitations = qualitativeDraft.ComparisonLimitations
        };
    }
}
