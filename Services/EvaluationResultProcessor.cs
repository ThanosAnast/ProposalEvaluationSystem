using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public sealed class EvaluationResultProcessor(IScoreCalculator scoreCalculator) : IEvaluationResultProcessor
{
    public EvaluationResult Process(EvaluationDraft draft, EvaluationRequest request, string modelName)
    {
        var definitions = request.Profile.Criteria.ToDictionary(definition => definition.Id, StringComparer.Ordinal);

        foreach (var criterion in draft.Criteria)
        {
            if (definitions.TryGetValue(criterion.Id, out var definition))
            {
                criterion.Name = definition.DisplayName;
            }
        }

        var calculation = scoreCalculator.Calculate(request.Profile, draft.Criteria);

        return new EvaluationResult
        {
            PromptTemplateUsed = request.PromptTemplateName,
            EvaluationProfileId = request.Profile.Id,
            ProgrammeType = request.Profile.ProgrammeType,
            ModelName = modelName,
            PromptContentSha256 = ContentHashService.ComputeSha256(request.PromptTemplateContent),
            InputFingerprint = request.InputFingerprint,
            ExecutiveSummary = draft.ExecutiveSummary,
            Criteria = draft.Criteria,
            TotalScore = calculation.TotalScore,
            ThresholdAssessment = calculation.ThresholdAssessment,
            FinalComment = draft.FinalComment,
            ConfidenceLevel = draft.ConfidenceLevel,
            Limitations = draft.Limitations,
            GeneratedPrompt = request.GeneratedPrompt
        };
    }
}
