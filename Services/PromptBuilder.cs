using System.Security;
using System.Text.RegularExpressions;
using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public sealed partial class PromptBuilder : IPromptBuilder
{
    public string Build(EvaluationRequest request)
    {
        var evaluationContext = WrapUntrustedDocument(
            "CALL_DOCUMENT",
            request.CallDocument.ExtractedText);

        var replacements = new Dictionary<string, string>
        {
            ["{{EVALUATION_CONTEXT}}"] = evaluationContext,
            ["{{CALL_TEXT}}"] = evaluationContext,
            ["{{PROPOSAL_TEXT}}"] = WrapUntrustedDocument("PROPOSAL_DOCUMENT", request.ProposalDocument.ExtractedText),
            ["{{PROGRAMME_TYPE}}"] = request.Profile.ProgrammeType.ToString(),
            ["{{EVALUATION_PROFILE}}"] = request.Profile.DisplayName
        };

        return TemplatePlaceholderPattern().Replace(
            request.PromptTemplateContent,
            match => replacements[match.Value]);
    }

    private static string WrapUntrustedDocument(string elementName, string content) =>
        $"<{elementName} untrusted=\"true\">\n{SecurityElement.Escape(content) ?? string.Empty}\n</{elementName}>";

    [GeneratedRegex("\\{\\{(?:EVALUATION_CONTEXT|CALL_TEXT|PROPOSAL_TEXT|PROGRAMME_TYPE|EVALUATION_PROFILE)\\}\\}", RegexOptions.CultureInvariant)]
    private static partial Regex TemplatePlaceholderPattern();
}
