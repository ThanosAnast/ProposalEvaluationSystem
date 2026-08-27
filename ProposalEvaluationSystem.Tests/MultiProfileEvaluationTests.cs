using ProposalEvaluationSystem.Models;
using ProposalEvaluationSystem.Services;
using Xunit;

namespace ProposalEvaluationSystem.Tests;

public sealed class MultiProfileEvaluationTests
{
    [Fact]
    public void ProfileCatalog_ContainsExactlyTheFiveEnabledProfilesAndPromptMappings()
    {
        var expected = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [EvaluationProfiles.HorizonEuropeRiaIaId] = "horizon_europe_ria_ia_2021_2025.md",
            [EvaluationProfiles.HorizonEuropeCsaId] = "horizon_europe_csa_2021_2025.md",
            [EvaluationProfiles.HorizonEuropeMscaStaffExchangesId] = "horizon_europe_msca_staff_exchanges_2024_2025.md",
            [EvaluationProfiles.Horizon2020RiaId] = "horizon_2020_ria_2020.md",
            [EvaluationProfiles.ErasmusCbheStrand2Id] = "erasmus_cbhe_strand_2_2025.md"
        };

        var profiles = new EvaluationProfileService().GetEnabledProfiles();

        Assert.Equal(expected.Count, profiles.Count);
        Assert.Equal(expected.Keys.Order(), profiles.Select(profile => profile.Id).Order());
        Assert.All(profiles, profile =>
        {
            Assert.True(profile.Enabled);
            Assert.True(profile.IsConfigured);
            Assert.Equal(expected[profile.Id], profile.PromptTemplateFileName);
            Assert.True(File.Exists(Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "..",
                "PromptTemplates",
                profile.PromptTemplateFileName))));
        });
        Assert.Equal(EvaluationProfiles.HorizonEuropeRiaIaId, EvaluationProfiles.DefaultId);
    }

    [Fact]
    public void AdditiveProfiles_CalculateTotalsAndBothKindsOfThresholds()
    {
        var calculator = new ScoreCalculator();
        var profile = EvaluationProfiles.HorizonEuropeRiaIa;

        var passing = calculator.Calculate(profile, CreateCriteria(profile, 3m, 4m, 3m));
        var individualFailure = calculator.Calculate(profile, CreateCriteria(profile, 2.5m, 4m, 4m));
        var horizon2020 = calculator.Calculate(
            EvaluationProfiles.Horizon2020Ria,
            CreateCriteria(EvaluationProfiles.Horizon2020Ria, 2.5m, 4m, 3m));

        Assert.Equal(10m, passing.TotalScore);
        Assert.Equal(15m, passing.ThresholdAssessment.MaximumTotalScore);
        Assert.Equal([3m, 4m, 3m], passing.Criteria.Select(item => item.RawScore).ToArray());
        Assert.Equal([3m, 4m, 3m], passing.Criteria.Select(item => item.ContributionToTotal).ToArray());
        Assert.All(passing.Criteria, item => Assert.True(item.ThresholdMet is true));
        Assert.True(passing.ThresholdAssessment.Passed);

        Assert.Equal(10.5m, individualFailure.TotalScore);
        Assert.False(individualFailure.ThresholdAssessment.IndividualThresholdsMet);
        Assert.True(individualFailure.ThresholdAssessment.OverallThresholdMet);
        Assert.False(individualFailure.ThresholdAssessment.Passed);

        Assert.Equal(9.5m, horizon2020.TotalScore);
        Assert.False(horizon2020.ThresholdAssessment.Passed);
    }

    [Fact]
    public void MscaWeightedScoring_UsesConfiguredWeightsOnTheHundredPointScale()
    {
        var calculator = new ScoreCalculator();
        var profile = EvaluationProfiles.HorizonEuropeMscaStaffExchanges;

        var result2024 = calculator.Calculate(profile, CreateCriteria(profile, 3.4m, 3.7m, 3.8m));
        var result2025 = calculator.Calculate(profile, CreateCriteria(profile, 4.5m, 3.7m, 4.5m));

        Assert.Equal(71.4m, result2024.TotalScore);
        Assert.Equal(100m, result2024.ThresholdAssessment.MaximumTotalScore);
        Assert.Equal([34m, 22.2m, 15.2m], result2024.Criteria.Select(item => item.ContributionToTotal).ToArray());
        Assert.Equal([50m, 30m, 20m], result2024.Criteria.Select(item => item.WeightPercentage).ToArray());
        Assert.All(result2024.Criteria, item => Assert.Null(item.ThresholdMet));
        Assert.True(result2024.ThresholdAssessment.IndividualThresholdsMet);
        Assert.True(result2024.ThresholdAssessment.Passed);
        Assert.Contains("weighted total score", result2024.ThresholdAssessment.Explanation, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(85.2m, result2025.TotalScore);
        Assert.True(result2025.ThresholdAssessment.Passed);
    }

    [Fact]
    public void MscaScoreResolution_IsCriterionAware()
    {
        var calculator = new ScoreCalculator();
        var profile = EvaluationProfiles.HorizonEuropeMscaStaffExchanges;

        Assert.True(calculator.IsValidScore(profile, EvaluationCriterionIds.Excellence, 3.4m));
        Assert.True(calculator.IsValidScore(profile, EvaluationCriterionIds.Excellence, 3.5m));
        Assert.False(calculator.IsValidScore(profile, EvaluationCriterionIds.Excellence, 3.45m));
    }

    [Fact]
    public void OfficialCsaReferenceScores_PreserveExactDecimalsWithoutRelaxingLlmValidation()
    {
        var calculator = new ScoreCalculator();
        var profile = EvaluationProfiles.HorizonEuropeCsa;

        Assert.False(calculator.IsValidScore(profile, EvaluationCriterionIds.Excellence, 2.6m));
        Assert.True(calculator.IsValidReferenceScore(profile, EvaluationCriterionIds.Excellence, 2.6m));
        Assert.False(calculator.IsValidReferenceScore(profile, EvaluationCriterionIds.Excellence, -0.01m));
        Assert.False(calculator.IsValidReferenceScore(profile, EvaluationCriterionIds.Excellence, 5.01m));
        Assert.Throws<ScoreValidationException>(() => calculator.Calculate(
            profile,
            CreateCriteria(profile, 2.6m, 1.9m, 2.9m)));
        Assert.Throws<ScoreValidationException>(() => calculator.CalculateReference(
            profile,
            CreateCriteria(profile, 5.01m, 3m, 3m)));

        var antero = calculator.CalculateReference(profile, CreateCriteria(profile, 2.6m, 1.9m, 2.9m));
        var hermes = calculator.CalculateReference(profile, CreateCriteria(profile, 4m, 3.5m, 4.25m));

        Assert.Equal(7.4m, antero.TotalScore);
        Assert.False(antero.ThresholdAssessment.Passed);
        Assert.Equal(11.75m, hermes.TotalScore);
        Assert.True(hermes.ThresholdAssessment.Passed);
    }

    [Fact]
    public void ErasmusScoring_UsesCriterionSpecificRangesAndThresholdsWithoutAnIncrement()
    {
        var calculator = new ScoreCalculator();
        var profile = EvaluationProfiles.ErasmusCbheStrand2;

        var passing = calculator.Calculate(profile, CreateCriteria(profile, 27m, 18m, 14m, 14m));
        var individualFailure = calculator.Calculate(profile, CreateCriteria(profile, 14m, 30m, 20m, 20m));

        Assert.Equal(73m, passing.TotalScore);
        Assert.Equal(100m, passing.ThresholdAssessment.MaximumTotalScore);
        Assert.True(passing.ThresholdAssessment.Passed);
        Assert.All(profile.Criteria, criterion => Assert.Null(criterion.ScoreIncrement));

        Assert.True(individualFailure.TotalScore >= profile.OverallThreshold);
        Assert.False(individualFailure.ThresholdAssessment.IndividualThresholdsMet);
        Assert.False(individualFailure.ThresholdAssessment.Passed);

        Assert.False(calculator.IsValidScore(profile, EvaluationCriterionIds.PartnershipCooperation, 21m));
        Assert.True(calculator.IsValidScore(profile, EvaluationCriterionIds.Relevance, 21m));
        Assert.Throws<ScoreValidationException>(() => calculator.Calculate(
            profile,
            CreateCriteria(profile, 27m, 18m, 21m, 14m)));
        Assert.Throws<ScoreValidationException>(() => new EvaluationResultProcessor(calculator).Process(
            CreateDraft(CreateCriteria(profile, 27m, 18m, 21m, 14m).ToList()),
            CreateRequest(profile),
            "test-model"));
    }

    [Fact]
    public void ProfileValidation_RejectsMalformedCriterionAndWeightedDefinitions()
    {
        var duplicateCriteria = new EvaluationProfile
        {
            Id = "duplicate",
            ProgrammeType = ProgrammeType.Horizon,
            DisplayName = "Duplicate",
            Enabled = true,
            PromptTemplateFileName = "prompt.md",
            ScoringMode = ScoringMode.Additive,
            OverallThreshold = 1m,
            Criteria =
            [
                Definition("same", 0m, 1m, 0.5m, 0.5m),
                Definition("same", 0m, 1m, 0.5m, 0.5m)
            ]
        };
        var missingWeight = new EvaluationProfile
        {
            Id = "weighted",
            ProgrammeType = ProgrammeType.Horizon,
            DisplayName = "Weighted",
            Enabled = true,
            PromptTemplateFileName = "prompt.md",
            ScoringMode = ScoringMode.WeightedPercentage,
            OverallThreshold = 1m,
            Criteria = [Definition("criterion", 0m, 5m, 0.1m)]
        };
        var invalidRange = new EvaluationProfile
        {
            Id = "range",
            ProgrammeType = ProgrammeType.Erasmus,
            DisplayName = "Range",
            Enabled = true,
            PromptTemplateFileName = "prompt.md",
            ScoringMode = ScoringMode.Additive,
            OverallThreshold = 1m,
            Criteria = [Definition("criterion", 5m, 4m, threshold: 6m)]
        };

        Assert.False(duplicateCriteria.IsConfigured);
        Assert.False(missingWeight.IsConfigured);
        Assert.False(invalidRange.IsConfigured);
    }

    [Theory]
    [InlineData(EvaluationProfiles.HorizonEuropeRiaIaId)]
    [InlineData(EvaluationProfiles.ErasmusCbheStrand2Id)]
    public void ResultProcessor_RejectsMissingDuplicateUnknownAndExtraCriteria(string profileId)
    {
        var profile = GetProfile(profileId);
        var processor = new EvaluationResultProcessor(new ScoreCalculator());
        var valid = CreateCriteria(profile, profile.Criteria.Select(ValidScore).ToArray()).ToList();

        AssertRejected(valid.Take(valid.Count - 1).ToList());

        var duplicate = valid.Select(Clone).ToList();
        duplicate[^1].CriterionId = duplicate[0].CriterionId;
        AssertRejected(duplicate);

        var unknown = valid.Select(Clone).ToList();
        unknown[^1].CriterionId = "unknown_criterion";
        AssertRejected(unknown);

        var extra = valid.Select(Clone).ToList();
        extra.Add(new CriterionEvaluation { CriterionId = "extra_criterion", Score = 0m });
        AssertRejected(extra);

        void AssertRejected(List<CriterionEvaluation> criteria) =>
            Assert.Throws<ScoreValidationException>(() => processor.Process(
                CreateDraft(criteria),
                CreateRequest(profile),
                "test-model"));
    }

    [Fact]
    public void ResultProcessor_MapsNamesAndOrdersCriteriaFromTheProfile()
    {
        var profile = EvaluationProfiles.ErasmusCbheStrand2;
        var reversed = CreateCriteria(profile, 27m, 18m, 14m, 14m).Reverse().ToList();

        var result = new EvaluationResultProcessor(new ScoreCalculator()).Process(
            CreateDraft(reversed),
            CreateRequest(profile),
            "test-model");

        Assert.Equal(profile.Criteria.Select(definition => definition.Id), result.Criteria.Select(item => item.CriterionId));
        Assert.Equal(profile.Criteria.Select(definition => definition.DisplayName), result.Criteria.Select(item => item.Name));
        Assert.Equal("InScope", result.ScopeAssessment.Status);
        Assert.Equal("Overall comment", result.OverallComment);
    }

    [Fact]
    public void ComparisonCalculator_HandlesWeightedMscaRawAndCalculatedTotalDifferences()
    {
        var profile = EvaluationProfiles.HorizonEuropeMscaStaffExchanges;
        var result = Compare(
            profile,
            [3.4m, 3.7m, 3.8m],
            [3.0m, 4.0m, 3.8m]);

        Assert.Equal(0.4m, result.Criteria[0].NumericDifference);
        Assert.Equal(-0.3m, result.Criteria[1].NumericDifference);
        Assert.Equal(71.4m, result.LlmTotalScore);
        Assert.Equal(69.2m, result.OfficialTotalScore);
        Assert.Equal(2.2m, result.TotalScoreDifference);
        Assert.Equal([30m, 24m, 15.2m], result.OfficialScoreBreakdown.Select(item => item.ContributionToTotal).ToArray());
        Assert.Equal([34m, 22.2m, 15.2m], result.LlmScoreBreakdown.Select(item => item.ContributionToTotal).ToArray());
        Assert.False(result.ThresholdAgreement);
    }

    [Fact]
    public void ComparisonCalculator_HandlesFourCriterionErasmusProfile()
    {
        var profile = EvaluationProfiles.ErasmusCbheStrand2;
        var result = Compare(
            profile,
            [27m, 18m, 14m, 14m],
            [25m, 20m, 12m, 13m]);

        Assert.Equal([2m, -2m, 2m, 1m], result.Criteria.Select(item => item.NumericDifference).ToArray());
        Assert.Equal(73m, result.LlmTotalScore);
        Assert.Equal(70m, result.OfficialTotalScore);
        Assert.Equal(3m, result.TotalScoreDifference);
        Assert.True(result.ThresholdAgreement);
    }

    [Fact]
    public void ComparisonAndCsvExport_PreserveExactOfficialCsaDecimals()
    {
        var profile = EvaluationProfiles.HorizonEuropeCsa;
        var comparison = Compare(profile, [4m, 4m, 4m], [4m, 3.5m, 4.25m]);
        var calculation = new ScoreCalculator().Calculate(profile, CreateCriteria(profile, 4m, 4m, 4m));
        var run = new ExperimentRun
        {
            Metadata = new ExperimentRunMetadata
            {
                DatasetId = "HERMES",
                RunId = "run-01",
                CreatedAtUtc = DateTimeOffset.Parse("2026-08-27T12:00:00Z"),
                EvaluationProfileId = profile.Id,
                ProgrammeType = profile.ProgrammeType,
                ModelName = OpenAiOptions.DefaultModel
            },
            IndependentEvaluation = new EvaluationResult
            {
                EvaluationProfileId = profile.Id,
                Criteria = CreateCriteria(profile, 4m, 4m, 4m).ToList(),
                TotalScore = calculation.TotalScore,
                ThresholdAssessment = calculation.ThresholdAssessment
            },
            ComparisonResult = comparison
        };

        var exporter = new ExperimentCsvExporter();
        var runsCsv = exporter.ExportExperimentRuns([run]);
        var criteriaCsv = exporter.ExportCriterionComparisons([run]);

        Assert.Equal(11.75m, comparison.OfficialTotalScore);
        Assert.Equal(4.25m, comparison.Criteria.Single(item =>
            item.CriterionId == EvaluationCriterionIds.Implementation).OfficialScore);
        Assert.Contains("\"11.75\"", runsCsv, StringComparison.Ordinal);
        Assert.Contains("\"4.25\"", criteriaCsv, StringComparison.Ordinal);
        Assert.DoesNotContain("\"11.8\"", runsCsv, StringComparison.Ordinal);
    }

    private static EvaluationComparisonResult Compare(
        EvaluationProfile profile,
        IReadOnlyList<decimal> independentScores,
        IReadOnlyList<decimal> officialScores)
    {
        var calculator = new ScoreCalculator();
        var independentCriteria = CreateCriteria(profile, independentScores.ToArray());
        var independentCalculation = calculator.Calculate(profile, independentCriteria);
        var independent = new EvaluationResult
        {
            EvaluationProfileId = profile.Id,
            Criteria = independentCriteria.ToList(),
            TotalScore = independentCalculation.TotalScore,
            ThresholdAssessment = independentCalculation.ThresholdAssessment
        };
        var reference = new ReferenceEvaluation
        {
            Criteria = profile.Criteria.Select((definition, index) => new ReferenceCriterionScore
            {
                CriterionId = definition.Id,
                OfficialScore = officialScores[index]
            }).ToList()
        };
        var qualitative = new ComparisonQualitativeDraft
        {
            Criteria = profile.Criteria.Select(definition => new CriterionComparisonQualitativeDraft
            {
                CriterionId = definition.Id,
                Summary = $"Comparison for {definition.Id}"
            }).ToList(),
            OverallComparisonSummary = "Overall comparison"
        };

        return new ComparisonCalculator(calculator).Calculate(profile, independent, reference, qualitative);
    }

    private static EvaluationProfile GetProfile(string profileId) =>
        EvaluationProfiles.All.Single(profile => profile.Id == profileId);

    private static IReadOnlyCollection<CriterionEvaluation> CreateCriteria(
        EvaluationProfile profile,
        params decimal[] scores)
    {
        Assert.Equal(profile.Criteria.Count, scores.Length);
        return profile.Criteria.Select((definition, index) => new CriterionEvaluation
        {
            CriterionId = definition.Id,
            Score = scores[index],
            Summary = $"Summary for {definition.Id}",
            Strengths = ["Strength"],
            Shortcomings = ["Shortcoming"],
            Evidence = ["Evidence"],
            Limitations = []
        }).ToArray();
    }

    private static EvaluationDraft CreateDraft(List<CriterionEvaluation> criteria) => new()
    {
        ScopeAssessment = new ScopeAssessment
        {
            Status = "InScope",
            Rationale = "The proposal addresses the context.",
            Evidence = ["Scope evidence"]
        },
        Criteria = criteria,
        OverallComment = "Overall comment",
        EvaluationLimitations = ["Evaluation limitation"]
    };

    private static EvaluationRequest CreateRequest(EvaluationProfile profile) => new()
    {
        Profile = profile,
        PromptTemplateName = profile.PromptTemplateFileName,
        PromptTemplateContent = "{{EVALUATION_CONTEXT}}\n{{PROPOSAL_TEXT}}",
        GeneratedPrompt = "Resolved prompt",
        InputFingerprint = "input-fingerprint"
    };

    private static CriterionEvaluation Clone(CriterionEvaluation source) => new()
    {
        CriterionId = source.CriterionId,
        Name = source.Name,
        Score = source.Score,
        Summary = source.Summary,
        Strengths = source.Strengths.ToList(),
        Shortcomings = source.Shortcomings.ToList(),
        Evidence = source.Evidence.ToList(),
        Limitations = source.Limitations.ToList()
    };

    private static decimal ValidScore(CriterionDefinition definition) =>
        definition.Threshold ?? definition.ScoreMinimum;

    private static CriterionDefinition Definition(
        string id,
        decimal minimum,
        decimal maximum,
        decimal? increment = null,
        decimal? threshold = null,
        decimal? weight = null) => new()
        {
            Id = id,
            DisplayName = id,
            ScoreMinimum = minimum,
            ScoreMaximum = maximum,
            ScoreIncrement = increment,
            Threshold = threshold,
            WeightPercentage = weight
        };
}
