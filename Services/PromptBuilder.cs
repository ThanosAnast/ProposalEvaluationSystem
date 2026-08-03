using System.Security;
using System.Text.RegularExpressions;
using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public sealed partial class PromptBuilder : IPromptBuilder
{
    public string Build(EvaluationRequest request)
    {
        var replacements = new Dictionary<string, string>
        {
            ["{{CALL_TEXT}}"] = WrapUntrustedDocument("CALL_DOCUMENT", request.CallDocument.ExtractedText),
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

    [GeneratedRegex("\\{\\{(?:CALL_TEXT|PROPOSAL_TEXT|PROGRAMME_TYPE|EVALUATION_PROFILE)\\}\\}", RegexOptions.CultureInvariant)]
    private static partial Regex TemplatePlaceholderPattern();
}
