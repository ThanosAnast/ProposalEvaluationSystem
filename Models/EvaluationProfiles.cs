namespace ProposalEvaluationSystem.Models;

public static class EvaluationProfiles
{
    public const string HorizonEuropeRiaIaId = "horizon-europe-ria-ia-2021-2025";
    public const string HorizonEuropeCsaId = "horizon-europe-csa-2021-2025";
    public const string HorizonEuropeMscaStaffExchangesId = "horizon-europe-msca-staff-exchanges-2024-2025";
    public const string Horizon2020RiaId = "horizon-2020-ria-2020";
    public const string ErasmusCbheStrand2Id = "erasmus-cbhe-strand-2-2025";
    public const string DefaultId = HorizonEuropeRiaIaId;

    public static EvaluationProfile HorizonEuropeRiaIa { get; } = new()
    {
        Id = HorizonEuropeRiaIaId,
        ProgrammeType = ProgrammeType.Horizon,
        DisplayName = "Horizon Europe RIA/IA (2021\u20132025)",
        Enabled = true,
        PromptTemplateFileName = "horizon_europe_ria_ia_2021_2025.md",
        ScoringMode = ScoringMode.Additive,
        OverallThreshold = 10m,
        Criteria = CreateStandardHorizonCriteria("Quality and Efficiency of Implementation")
    };

    public static EvaluationProfile HorizonEuropeCsa { get; } = new()
    {
        Id = HorizonEuropeCsaId,
        ProgrammeType = ProgrammeType.Horizon,
        DisplayName = "Horizon Europe CSA (2021\u20132025)",
        Enabled = true,
        PromptTemplateFileName = "horizon_europe_csa_2021_2025.md",
        ScoringMode = ScoringMode.Additive,
        OverallThreshold = 10m,
        Criteria = CreateStandardHorizonCriteria("Quality and Efficiency of Implementation")
    };

    public static EvaluationProfile HorizonEuropeMscaStaffExchanges { get; } = new()
    {
        Id = HorizonEuropeMscaStaffExchangesId,
        ProgrammeType = ProgrammeType.Horizon,
        DisplayName = "Horizon Europe MSCA Staff Exchanges (2024\u20132025)",
        Enabled = true,
        PromptTemplateFileName = "horizon_europe_msca_staff_exchanges_2024_2025.md",
        ScoringMode = ScoringMode.WeightedPercentage,
        OverallThreshold = 70m,
        Criteria =
        [
            CreateCriterion(EvaluationCriterionIds.Excellence, "Excellence", 0m, 5m, 0.1m, weightPercentage: 50m),
            CreateCriterion(EvaluationCriterionIds.Impact, "Impact", 0m, 5m, 0.1m, weightPercentage: 30m),
            CreateCriterion(
                EvaluationCriterionIds.Implementation,
                "Quality and Efficiency of the Implementation",
                0m,
                5m,
                0.1m,
                weightPercentage: 20m)
        ]
    };

    public static EvaluationProfile Horizon2020Ria { get; } = new()
    {
        Id = Horizon2020RiaId,
        ProgrammeType = ProgrammeType.Horizon,
        DisplayName = "Horizon 2020 RIA (2020)",
        Enabled = true,
        PromptTemplateFileName = "horizon_2020_ria_2020.md",
        ScoringMode = ScoringMode.Additive,
        OverallThreshold = 10m,
        Criteria = CreateStandardHorizonCriteria("Quality and Efficiency of Implementation")
    };

    public static EvaluationProfile ErasmusCbheStrand2 { get; } = new()
    {
        Id = ErasmusCbheStrand2Id,
        ProgrammeType = ProgrammeType.Erasmus,
        DisplayName = "Erasmus+ CBHE Strand 2 (2025)",
        Enabled = true,
        PromptTemplateFileName = "erasmus_cbhe_strand_2_2025.md",
        ScoringMode = ScoringMode.Additive,
        OverallThreshold = 60m,
        Criteria =
        [
            CreateCriterion(EvaluationCriterionIds.Relevance, "Relevance of the Project", 0m, 30m, threshold: 15m),
            CreateCriterion(
                EvaluationCriterionIds.ProjectDesignImplementation,
                "Quality of the Project Design and Implementation",
                0m,
                30m,
                threshold: 15m),
            CreateCriterion(
                EvaluationCriterionIds.PartnershipCooperation,
                "Quality of the Partnership and Cooperation Arrangements",
                0m,
                20m,
                threshold: 10m),
            CreateCriterion(
                EvaluationCriterionIds.ImpactSustainabilityDissemination,
                "Sustainability, Impact and Dissemination of the Expected Results",
                0m,
                20m,
                threshold: 10m)
        ]
    };

    public static IReadOnlyList<EvaluationProfile> All { get; } =
    [
        HorizonEuropeRiaIa,
        HorizonEuropeCsa,
        HorizonEuropeMscaStaffExchanges,
        Horizon2020Ria,
        ErasmusCbheStrand2
    ];

    private static IReadOnlyList<CriterionDefinition> CreateStandardHorizonCriteria(string implementationDisplayName) =>
    [
        CreateCriterion(EvaluationCriterionIds.Excellence, "Excellence", 0m, 5m, 0.5m, 3m),
        CreateCriterion(EvaluationCriterionIds.Impact, "Impact", 0m, 5m, 0.5m, 3m),
        CreateCriterion(
            EvaluationCriterionIds.Implementation,
            implementationDisplayName,
            0m,
            5m,
            0.5m,
            3m)
    ];

    private static CriterionDefinition CreateCriterion(
        string id,
        string displayName,
        decimal scoreMinimum,
        decimal scoreMaximum,
        decimal? scoreIncrement = null,
        decimal? threshold = null,
        decimal? weightPercentage = null) => new()
        {
            Id = id,
            DisplayName = displayName,
            ScoreMinimum = scoreMinimum,
            ScoreMaximum = scoreMaximum,
            ScoreIncrement = scoreIncrement,
            Threshold = threshold,
            WeightPercentage = weightPercentage
        };
}
