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
            ["{{REAL_EVALUATION_TEXT}}"] = string.IsNullOrWhiteSpace(request.RealEvaluationDocument?.ExtractedText)
                ? "Not provided."
                : request.RealEvaluationDocument.ExtractedText,
            ["{{PROGRAMME_TYPE}}"] = request.ProgrammeType.ToString(),
            ["{{EVALUATION_LEVEL}}"] = request.EvaluationLevel.ToString(),
            ["{{OUTPUT_TEMPLATE}}"] = GetOutputTemplate()
        };

        var prompt = request.PromptTemplateContent;

        foreach (var replacement in replacements)
        {
            prompt = prompt.Replace(replacement.Key, replacement.Value, StringComparison.Ordinal);
        }

        return prompt;
    }

    private static string GetOutputTemplate() =>
        """
        Return a structured evaluation report with:
        - Prompt template used
        - Programme type
        - Evaluation level
        - Executive summary
        - Score table for Excellence, Impact, Quality and Efficiency of Implementation, Total score, Threshold result
        - Detailed criterion sections with score, strengths, weaknesses, evidence, and assessment
        - Final comment
        - Confidence level
        - Limitations
        - Placeholder for future comparison with the real ESR
        """;
}
