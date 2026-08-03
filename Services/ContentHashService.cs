using System.Security.Cryptography;
using System.Text;
using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public static class ContentHashService
{
    public static string ComputeSha256(string? content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content ?? string.Empty));
        return Convert.ToHexStringLower(bytes);
    }

    public static string ComputeInputFingerprint(
        ProcessedDocument callDocument,
        ProcessedDocument proposalDocument,
        string profileId,
        string promptContent)
    {
        var canonical = string.Join(
            '\n',
            ComputeSha256(callDocument.ExtractedText),
            ComputeSha256(proposalDocument.ExtractedText),
            profileId,
            ComputeSha256(promptContent));

        return ComputeSha256(canonical);
    }
}
