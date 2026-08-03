using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public class PromptBuilder : IPromptBuilder
{
    public string Build(EvaluationRequest request)
    {
        var replacements = new Dictionary<string, string>
        {
            ["{{CALL_TEXT}}"] = request.CallDocument.ExtractedText,
            ["{{PROPOSAL_TEXT}}"] = request.ProposalDocument.ExtractedText,
            ["{{PROGRAMME_TYPE}}"] = request.Profile.ProgrammeType.ToString(),
            ["{{EVALUATION_PROFILE}}"] = request.Profile.DisplayName
        };

        var prompt = request.PromptTemplateContent;

        foreach (var replacement in replacements)
        {
            prompt = prompt.Replace(replacement.Key, replacement.Value, StringComparison.Ordinal);
        }

        return prompt;
    }
}
