namespace ProposalEvaluationSystem.Services;

public sealed class OpenAiServiceException : Exception
{
    public OpenAiServiceException(string message)
        : base(message)
    {
    }

    public OpenAiServiceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
