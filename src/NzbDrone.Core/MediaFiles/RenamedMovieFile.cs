namespace NzbDrone.Core.MediaFiles
{
    public class RenamedMovieFile
    {
        public MediaFile MovieFile { get; set; }
        public string PreviousPath { get; set; }
        public string PreviousRelativePath { get; set; }
    }
}
