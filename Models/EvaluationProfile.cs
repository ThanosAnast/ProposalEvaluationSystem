namespace ProposalEvaluationSystem.Models;

public sealed class EvaluationProfile
{
    public required string Id { get; init; }

    public required ProgrammeType ProgrammeType { get; init; }

    public required string DisplayName { get; init; }

    public bool Enabled { get; init; }

    public string? PromptTemplateFileName { get; init; }

    public IReadOnlyList<CriterionDefinition> Criteria { get; init; } = [];

    public decimal? ScoreMinimum { get; init; }

    public decimal? ScoreMaximum { get; init; }

    public decimal? ScoreIncrement { get; init; }

    public decimal? OverallThreshold { get; init; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(PromptTemplateFileName) &&
        Criteria.Count > 0 &&
        ScoreMinimum.HasValue &&
        ScoreMaximum.HasValue &&
        ScoreIncrement.HasValue &&
        OverallThreshold.HasValue;

    public decimal MaximumTotal => IsConfigured
        ? Criteria.Count * ScoreMaximum!.Value
        : 0m;
}
