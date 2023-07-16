using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public interface IDecisionEngineSpecification
    {
        RejectionType Type { get; }

        SpecificationPriority Priority { get; }

        Decision IsSatisfiedBy(RemoteEpisode subject, SceneSearchCriteriaBase searchCriteria);
        Decision IsSatisfiedBy(RemoteMovie subject, MovieSearchCriteria searchCriteria);
    }
}
