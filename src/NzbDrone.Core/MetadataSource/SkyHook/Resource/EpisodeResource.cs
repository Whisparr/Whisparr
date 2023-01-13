using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class EpisodeResource
    {
        public int ForeignId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public int? AbsoluteEpisodeNumber { get; set; }
        public int? AiredAfterSeasonNumber { get; set; }
        public int? AiredBeforeSeasonNumber { get; set; }
        public int? AiredBeforeEpisodeNumber { get; set; }
        public string Title { get; set; }
        public string ReleaseDate { get; set; }
        public string Overview { get; set; }
        public string Image { get; set; }
        public List<ActorResource> Credits { get; set; }
    }
}
