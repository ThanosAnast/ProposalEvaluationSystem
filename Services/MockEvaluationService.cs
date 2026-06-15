using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public class MockEvaluationService : IEvaluationService
{
    public Task<EvaluationResult> EvaluateAsync(EvaluationRequest request, CancellationToken cancellationToken = default)
    {
        var result = new EvaluationResult
        {
            PromptTemplateUsed = request.PromptTemplateName,
            ProgrammeType = request.ProgrammeType,
            EvaluationLevel = request.EvaluationLevel,
            ExecutiveSummary = "The proposal addresses a relevant research and innovation challenge and presents a credible intervention logic, but the excellence and implementation sections require stronger methodological detail, clearer risk handling, and more explicit evidence for feasibility.",
            Excellence = new CriterionEvaluation
            {
                Name = "Excellence",
                Score = 2.5m,
                Strengths =
                [
                    "The proposal identifies a timely scientific challenge aligned with the call topic.",
                    "The objectives are generally understandable and connected to expected project outputs."
                ],
                Weaknesses =
                [
                    "The methodology remains broad and does not sufficiently justify key research choices.",
                    "The state-of-the-art analysis is present but not consistently linked to measurable advances."
                ],
                Evidence =
                [
                    "The work plan references validation activities but gives limited detail on success criteria.",
                    "Several claims of novelty are not supported by specific literature or benchmark comparisons."
                ],
                Assessment = "The excellence criterion is partially addressed. The proposal has a plausible concept but lacks the depth and rigor expected for a higher Horizon-style score."
            },
            Impact = new CriterionEvaluation
            {
                Name = "Impact",
                Score = 4.0m,
                Strengths =
                [
                    "Expected outcomes are well aligned with the call and address identifiable stakeholder needs.",
                    "The dissemination and exploitation pathway is credible for an experimental research project."
                ],
                Weaknesses =
                [
                    "Some impact indicators are qualitative and would benefit from clearer baselines.",
                    "The route from research outputs to longer-term adoption is not fully evidenced."
                ],
                Evidence =
                [
                    "The proposal includes target groups, communication channels, and indicative exploitation actions.",
                    "The impact section links project outputs to policy and market relevance."
                ],
                Assessment = "The impact criterion is comparatively strong. The proposal offers a convincing impact narrative, though measurable indicators and adoption assumptions could be sharpened."
            },
            Implementation = new CriterionEvaluation
            {
                Name = "Quality and Efficiency of Implementation",
                Score = 3.0m,
                Strengths =
                [
                    "The work package structure is coherent and broadly suitable for the proposed activities.",
                    "Roles and responsibilities are described at a level sufficient for a prototype evaluation."
                ],
                Weaknesses =
                [
                    "Risk management is generic and does not fully address technical dependencies.",
                    "Resource allocation and timing need stronger justification."
                ],
                Evidence =
                [
                    "The Gantt-style sequencing is plausible but lacks detail on dependencies between tasks.",
                    "Management procedures are described, but contingency planning is limited."
                ],
                Assessment = "Implementation is adequate but not fully convincing. The plan is workable, yet several execution risks and resource assumptions remain underdeveloped."
            },
            TotalScore = 9.5m,
            ThresholdAssessment = new ThresholdAssessment
            {
                TotalScore = 9.5m,
                RequiredTotalScore = 10.0m,
                IndividualThresholdsMet = false,
                OverallResult = "Below threshold",
                Explanation = "The total score is below the indicative Horizon threshold, and Excellence does not reach a convincing level for funding recommendation."
            },
            FinalComment = "The proposal shows promise and relevance, especially in its impact logic, but should strengthen methodological credibility, evidence of novelty, risk planning, and implementation detail before resubmission.",
            ConfidenceLevel = "Medium",
            Limitations =
            [
                "This is a mock evaluation and does not call an LLM.",
                "Scores are fixed for prototype demonstration.",
                "Document parsing quality depends on the uploaded file text layer."
            ],
            GeneratedPrompt = request.GeneratedPrompt,
            RealEvaluationComparisonPlaceholder = "Future version: compare the mock/LLM evaluation against the uploaded ESR and highlight score, evidence, and reasoning differences."
        };

        return Task.FromResult(result);
    }
}
