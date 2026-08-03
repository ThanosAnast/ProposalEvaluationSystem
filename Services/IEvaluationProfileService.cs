using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public interface IEvaluationProfileService
{
    IReadOnlyList<EvaluationProfile> GetAllProfiles();

    IReadOnlyList<EvaluationProfile> GetEnabledProfiles();

    EvaluationProfile GetRequiredEnabledProfile(string profileId);
}
