using System;

namespace NzbDrone.Core.Parser.Model
{
    public class ImportListItemInfo
    {
        public int ImportListId { get; set; }
        public string ImportList { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public int TpdbSiteId { get; set; }
        public int TpdbEpisodeId { get; set; }
        public DateTime ReleaseDate { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", ReleaseDate, Title);
        }
    }
}
