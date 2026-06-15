using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public class OpenAiEvaluationService(HttpClient httpClient, IOptions<OpenAiOptions> options) : IEvaluationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly OpenAiOptions openAiOptions = options.Value;

    public async Task<EvaluationResult> EvaluateAsync(EvaluationRequest request, CancellationToken cancellationToken = default)
    {
        var apiKey = openAiOptions.GetApiKey();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "OpenAI evaluation is selected, but no API key is configured. Set user secret 'OpenAI:ApiKey' or environment variable 'OPENAI_API_KEY'.");
        }

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, openAiOptions.Endpoint);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Content = JsonContent.Create(CreateRequestPayload(request));

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"OpenAI API request failed with {(int)response.StatusCode} {response.ReasonPhrase}: {responseBody}");
        }

        var jsonResult = ExtractOutputText(responseBody);
        var modelResult = JsonSerializer.Deserialize<OpenAiEvaluationResponse>(jsonResult, JsonOptions)
            ?? throw new InvalidOperationException("OpenAI returned an empty evaluation result.");

        return modelResult.ToEvaluationResult(request);
    }

    private object CreateRequestPayload(EvaluationRequest request) => new
    {
        model = openAiOptions.Model,
        input = new object[]
        {
            new
            {
                role = "system",
                content = new object[]
                {
                    new
                    {
                        type = "input_text",
                        text = GetSystemInstructions()
                    }
                }
            },
            new
            {
                role = "user",
                content = new object[]
                {
                    new
                    {
                        type = "input_text",
                        text = BuildUserPrompt(request)
                    }
                }
            }
        },
        text = new
        {
            format = new
            {
                type = "json_schema",
                name = "proposal_evaluation_result",
                strict = true,
                schema = GetResponseSchema()
            }
        },
        max_output_tokens = openAiOptions.MaxOutputTokens
    };

    private static string GetSystemInstructions() =>
        """
        You are an expert evaluator of technical and research proposals.
        Produce an independent, evidence-based evaluation from the supplied call and proposal text.
        Do not treat a provided ESR as ground truth; use it only as optional comparison context.
        Use scores from 0.0 to 5.0 for each main criterion.
        Compute totalScore as Excellence + Impact + Implementation.
        For Horizon-style assessments, a typical funding threshold is 10.0 total and 3.0 per criterion unless the prompt states otherwise.
        Return only JSON that matches the required schema.
        """;

    private static string BuildUserPrompt(EvaluationRequest request)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Evaluation metadata:");
        builder.AppendLine($"- Prompt template used: {request.PromptTemplateName}");
        builder.AppendLine($"- Programme type: {request.ProgrammeType}");
        builder.AppendLine($"- Evaluation level: {request.EvaluationLevel}");
        builder.AppendLine();
        builder.AppendLine("Final prompt generated from the selected template:");
        builder.AppendLine(request.GeneratedPrompt);

        return builder.ToString();
    }

    private static object GetResponseSchema() => new
    {
        type = "object",
        additionalProperties = false,
        required = new[]
        {
            "executiveSummary",
            "excellence",
            "impact",
            "implementation",
            "totalScore",
            "thresholdAssessment",
            "finalComment",
            "confidenceLevel",
            "limitations",
            "realEvaluationComparisonPlaceholder"
        },
        properties = new
        {
            executiveSummary = new { type = "string" },
            excellence = GetCriterionSchema(),
            impact = GetCriterionSchema(),
            implementation = GetCriterionSchema(),
            totalScore = new { type = "number" },
            thresholdAssessment = new
            {
                type = "object",
                additionalProperties = false,
                required = new[] { "totalScore", "requiredTotalScore", "individualThresholdsMet", "overallResult", "explanation" },
                properties = new
                {
                    totalScore = new { type = "number" },
                    requiredTotalScore = new { type = "number" },
                    individualThresholdsMet = new { type = "boolean" },
                    overallResult = new { type = "string" },
                    explanation = new { type = "string" }
                }
            },
            finalComment = new { type = "string" },
            confidenceLevel = new { type = "string" },
            limitations = new
            {
                type = "array",
                items = new { type = "string" }
            },
            realEvaluationComparisonPlaceholder = new { type = "string" }
        }
    };

    private static object GetCriterionSchema() => new
    {
        type = "object",
        additionalProperties = false,
        required = new[] { "score", "strengths", "weaknesses", "evidence", "assessment" },
        properties = new
        {
            score = new { type = "number" },
            strengths = new
            {
                type = "array",
                items = new { type = "string" }
            },
            weaknesses = new
            {
                type = "array",
                items = new { type = "string" }
            },
            evidence = new
            {
                type = "array",
                items = new { type = "string" }
            },
            assessment = new { type = "string" }
        }
    };

    private static string ExtractOutputText(string responseBody)
    {
        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;

        if (root.TryGetProperty("output_text", out var outputText) && outputText.ValueKind == JsonValueKind.String)
        {
            return outputText.GetString() ?? string.Empty;
        }

        if (!root.TryGetProperty("output", out var outputItems) || outputItems.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("OpenAI response did not contain output text.");
        }

        var builder = new StringBuilder();

        foreach (var item in outputItems.EnumerateArray())
        {
            if (!item.TryGetProperty("content", out var contentItems) || contentItems.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var contentItem in contentItems.EnumerateArray())
            {
                if (contentItem.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                {
                    builder.Append(text.GetString());
                }
            }
        }

        var result = builder.ToString();

        if (string.IsNullOrWhiteSpace(result))
        {
            throw new InvalidOperationException("OpenAI response output text was empty.");
        }

        return result;
    }

    private sealed class OpenAiEvaluationResponse
    {
        public string ExecutiveSummary { get; set; } = string.Empty;

        public OpenAiCriterionResponse Excellence { get; set; } = new();

        public OpenAiCriterionResponse Impact { get; set; } = new();

        public OpenAiCriterionResponse Implementation { get; set; } = new();

        public decimal TotalScore { get; set; }

        public ThresholdAssessment ThresholdAssessment { get; set; } = new();

        public string FinalComment { get; set; } = string.Empty;

        public string ConfidenceLevel { get; set; } = string.Empty;

        public List<string> Limitations { get; set; } = [];

        public string RealEvaluationComparisonPlaceholder { get; set; } = string.Empty;

        public EvaluationResult ToEvaluationResult(EvaluationRequest request) => new()
        {
            PromptTemplateUsed = request.PromptTemplateName,
            ProgrammeType = request.ProgrammeType,
            EvaluationLevel = request.EvaluationLevel,
            ExecutiveSummary = ExecutiveSummary,
            Excellence = Excellence.ToCriterion("Excellence"),
            Impact = Impact.ToCriterion("Impact"),
            Implementation = Implementation.ToCriterion("Quality and Efficiency of Implementation"),
            TotalScore = TotalScore,
            ThresholdAssessment = ThresholdAssessment,
            FinalComment = FinalComment,
            ConfidenceLevel = ConfidenceLevel,
            Limitations = Limitations,
            GeneratedPrompt = request.GeneratedPrompt,
            RealEvaluationComparisonPlaceholder = RealEvaluationComparisonPlaceholder
        };
    }

    private sealed class OpenAiCriterionResponse
    {
        public decimal Score { get; set; }

        public List<string> Strengths { get; set; } = [];

        public List<string> Weaknesses { get; set; } = [];

        public List<string> Evidence { get; set; } = [];

        public string Assessment { get; set; } = string.Empty;

        public CriterionEvaluation ToCriterion(string name) => new()
        {
            Name = name,
            Score = Score,
            Strengths = Strengths,
            Weaknesses = Weaknesses,
            Evidence = Evidence,
            Assessment = Assessment
        };
    }
}
