namespace ProposalEvaluationSystem.Services;

public sealed class ScoreValidationException(string message) : InvalidOperationException(message);
