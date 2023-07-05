using System;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Tv
{
    public class Episode : ModelBase, IComparable
    {
        public Episode()
        {
            Images = new List<MediaCover.MediaCover>();
            Actors = new List<Actor>();
        }

        public const string AIR_DATE_FORMAT = "yyyy-MM-dd";

        public int SeriesId { get; set; }
        public int TvdbId { get; set; }
        public int EpisodeFileId { get; set; }
        public int SeasonNumber { get; set; }
        public string Title { get; set; }
        public string AirDate { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public string Overview { get; set; }
        public int Runtime { get; set; }
        public bool Monitored { get; set; }
        public int? AbsoluteEpisodeNumber { get; set; }
        public Ratings Ratings { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public List<Actor> Actors { get; set; }
        public DateTime? LastSearchTime { get; set; }

        public string SeriesTitle { get; private set; }

        public LazyLoaded<EpisodeFile> EpisodeFile { get; set; }

        public Series Series { get; set; }

        public bool HasFile => EpisodeFileId > 0;

        public override string ToString()
        {
            return string.Format("[{0}]{1} {2}", Id, AirDate, Title.NullSafe());
        }

        public int CompareTo(object obj)
        {
            var other = (Episode)obj;

            if (SeasonNumber > other.SeasonNumber)
            {
                return 1;
            }

            if (SeasonNumber < other.SeasonNumber)
            {
                return -1;
            }

            return 0;
        }
    }
}
