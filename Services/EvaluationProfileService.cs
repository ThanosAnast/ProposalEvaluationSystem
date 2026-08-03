using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public sealed class EvaluationProfileService : IEvaluationProfileService
{
    public IReadOnlyList<EvaluationProfile> GetAllProfiles() => EvaluationProfiles.All;

    public IReadOnlyList<EvaluationProfile> GetEnabledProfiles() =>
        EvaluationProfiles.All.Where(profile => profile.Enabled && profile.IsConfigured).ToArray();

    public EvaluationProfile GetRequiredEnabledProfile(string profileId)
    {
        var profile = EvaluationProfiles.All.FirstOrDefault(
            candidate => string.Equals(candidate.Id, profileId, StringComparison.Ordinal));

        if (profile is null)
        {
            throw new InvalidOperationException($"Evaluation profile '{profileId}' does not exist.");
        }

        if (!profile.Enabled || !profile.IsConfigured)
        {
            throw new InvalidOperationException($"Evaluation profile '{profileId}' is not enabled or configured.");
        }

        return profile;
    }
}
