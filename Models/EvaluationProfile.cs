namespace ProposalEvaluationSystem.Models;

public enum ScoringMode
{
    Additive,
    WeightedPercentage
}

public sealed class EvaluationProfile
{
    public required string Id { get; init; }

    public required ProgrammeType ProgrammeType { get; init; }

    public required string DisplayName { get; init; }

    public bool Enabled { get; init; }

    public string PromptTemplateFileName { get; init; } = string.Empty;

    public ScoringMode ScoringMode { get; init; }

    public IReadOnlyList<CriterionDefinition> Criteria { get; init; } = [];

    public decimal OverallThreshold { get; init; }

    public decimal MaximumTotal => ScoringMode switch
    {
        ScoringMode.Additive => Criteria.Sum(criterion => criterion.ScoreMaximum),
        ScoringMode.WeightedPercentage => Criteria.Sum(criterion => criterion.WeightPercentage ?? 0m),
        _ => 0m
    };

    public bool IsConfigured
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Id) ||
                string.IsNullOrWhiteSpace(DisplayName) ||
                string.IsNullOrWhiteSpace(PromptTemplateFileName) ||
                Criteria.Count == 0 ||
                !Enum.IsDefined(ScoringMode))
            {
                return false;
            }

            var criterionIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var criterion in Criteria)
            {
                if (string.IsNullOrWhiteSpace(criterion.Id) ||
                    string.IsNullOrWhiteSpace(criterion.DisplayName) ||
                    !criterionIds.Add(criterion.Id) ||
                    criterion.ScoreMaximum < criterion.ScoreMinimum ||
                    criterion.ScoreIncrement is <= 0m ||
                    criterion.Threshold < criterion.ScoreMinimum ||
                    criterion.Threshold > criterion.ScoreMaximum)
                {
                    return false;
                }

                if (ScoringMode == ScoringMode.WeightedPercentage &&
                    (criterion.ScoreMaximum <= 0m || criterion.WeightPercentage is null or <= 0m))
                {
                    return false;
                }
            }

            return OverallThreshold >= 0m && OverallThreshold <= MaximumTotal;
        }
    }
}
