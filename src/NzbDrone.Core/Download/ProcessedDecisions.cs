using System.Collections.Generic;
using NzbDrone.Core.DecisionEngine;

namespace NzbDrone.Core.Download
{
    public class ProcessedDecisions
    {
        public List<SceneDownloadDecision> Grabbed { get; set; }
        public List<SceneDownloadDecision> Pending { get; set; }
        public List<SceneDownloadDecision> Rejected { get; set; }

        public ProcessedDecisions(List<SceneDownloadDecision> grabbed, List<SceneDownloadDecision> pending, List<SceneDownloadDecision> rejected)
        {
            Grabbed = grabbed;
            Pending = pending;
            Rejected = rejected;
        }
    }
}
