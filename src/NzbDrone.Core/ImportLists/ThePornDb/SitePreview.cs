namespace NzbDrone.Core.ImportLists.TPDb
{
    public class SitePreview
    {
        public int TvdbId { get; set; }
        public string Title { get; set; }
        public bool Exists { get; set; }
        public int SceneCount { get; set; }
    }
}
