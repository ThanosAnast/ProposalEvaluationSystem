namespace ProposalEvaluationSystem.Services;

public sealed class ExperimentsOptions
{
    public const string DefaultSchemaVersion = "1.0";

    public bool StoreSensitiveContent { get; set; }

    public string? GitCommitSha { get; set; }

    public string ExperimentSchemaVersion { get; set; } = DefaultSchemaVersion;
}
