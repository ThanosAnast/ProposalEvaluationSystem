using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public sealed class EvaluationResultProcessor(IScoreCalculator scoreCalculator) : IEvaluationResultProcessor
{
    public EvaluationResult Process(EvaluationDraft draft, EvaluationRequest request, string modelName)
    {
        ArgumentNullException.ThrowIfNull(draft);
        ArgumentNullException.ThrowIfNull(request);

        var definitions = request.Profile.Criteria.ToDictionary(definition => definition.Id, StringComparer.Ordinal);
        if (draft.Criteria.Any(criterion => string.IsNullOrWhiteSpace(criterion.CriterionId)))
        {
            throw new ScoreValidationException("Every evaluation criterion must have a criterion ID.");
        }

        var groupedCriteria = draft.Criteria
            .GroupBy(criterion => criterion.CriterionId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
        if (groupedCriteria.Any(group => group.Value.Length != 1))
        {
            throw new ScoreValidationException("The evaluation contains a duplicate criterion ID.");
        }

        var unknownIds = groupedCriteria.Keys.Where(criterionId => !definitions.ContainsKey(criterionId)).ToArray();
        if (unknownIds.Length > 0)
        {
            throw new ScoreValidationException(
                $"The evaluation contains an unknown criterion ID: {string.Join(", ", unknownIds)}.");
        }

        var missingIds = definitions.Keys.Where(criterionId => !groupedCriteria.ContainsKey(criterionId)).ToArray();
        if (missingIds.Length > 0)
        {
            throw new ScoreValidationException(
                $"The evaluation is missing a required criterion: {string.Join(", ", missingIds)}.");
        }

        if (draft.Criteria.Count != request.Profile.Criteria.Count)
        {
            throw new ScoreValidationException("The evaluation must contain exactly one score for every profile criterion.");
        }

        var orderedCriteria = request.Profile.Criteria
            .Select(definition =>
            {
                var criterion = groupedCriteria[definition.Id][0];
                criterion.Name = definition.DisplayName;
                return criterion;
            })
            .ToList();
        var calculation = scoreCalculator.Calculate(request.Profile, orderedCriteria);

        return new EvaluationResult
        {
            PromptTemplateUsed = request.PromptTemplateName,
            EvaluationProfileId = request.Profile.Id,
            ProgrammeType = request.Profile.ProgrammeType,
            ModelName = modelName,
            PromptContentSha256 = ContentHashService.ComputeSha256(request.PromptTemplateContent),
            InputFingerprint = request.InputFingerprint,
            ScopeAssessment = draft.ScopeAssessment,
            Criteria = orderedCriteria,
            TotalScore = calculation.TotalScore,
            ThresholdAssessment = calculation.ThresholdAssessment,
            OverallComment = draft.OverallComment,
            EvaluationLimitations = draft.EvaluationLimitations,
            GeneratedPrompt = request.GeneratedPrompt
        };
    }
}
