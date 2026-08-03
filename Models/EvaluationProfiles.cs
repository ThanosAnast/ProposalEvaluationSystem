namespace ProposalEvaluationSystem.Models;

public static class EvaluationProfiles
{
    public const string HorizonEssentialId = "horizon-essential-v3";
    public const string ErasmusUnconfiguredId = "erasmus-unconfigured";

    public static EvaluationProfile HorizonEssential { get; } = new()
    {
        Id = HorizonEssentialId,
        ProgrammeType = ProgrammeType.Horizon,
        DisplayName = "Horizon Essential",
        Enabled = true,
        PromptTemplateFileName = "horizon_essential_v3.md",
        ScoreMinimum = 0m,
        ScoreMaximum = 5m,
        ScoreIncrement = 0.5m,
        OverallThreshold = 10m,
        Criteria =
        [
            new CriterionDefinition
            {
                Id = EvaluationCriterionIds.Excellence,
                DisplayName = "Excellence",
                Threshold = 3m
            },
            new CriterionDefinition
            {
                Id = EvaluationCriterionIds.Impact,
                DisplayName = "Impact",
                Threshold = 3m
            },
            new CriterionDefinition
            {
                Id = EvaluationCriterionIds.Implementation,
                DisplayName = "Quality and Efficiency of Implementation",
                Threshold = 3m
            }
        ]
    };

    public static EvaluationProfile ErasmusUnconfigured { get; } = new()
    {
        Id = ErasmusUnconfiguredId,
        ProgrammeType = ProgrammeType.Erasmus,
        DisplayName = "Erasmus (criteria not configured)",
        Enabled = false
    };

    public static IReadOnlyList<EvaluationProfile> All { get; } =
    [
        HorizonEssential,
        ErasmusUnconfigured
    ];
}
