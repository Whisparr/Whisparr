using System;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Profiles;

namespace NzbDrone.Core.Movies
{
    public class Media : ModelBase
    {
        public Media()
        {
            Tags = new HashSet<int>();
            MediaMetadata = new MediaMetadata();
        }

        public int MovieMetadataId { get; set; }

        public bool Monitored { get; set; }
        public MovieStatusType MinimumAvailability { get; set; }
        public int ProfileId { get; set; }

        public string Path { get; set; }

        public LazyLoaded<MediaMetadata> MediaMetadata { get; set; }

        public string RootFolderPath { get; set; }
        public DateTime Added { get; set; }
        public Profile Profile { get; set; }
        public HashSet<int> Tags { get; set; }
        public AddMovieOptions AddOptions { get; set; }
        public MediaFile MovieFile { get; set; }
        public int MovieFileId { get; set; }

        public bool HasFile => MovieFileId > 0;

        //compatibility properties
        public string Title
        {
            get { return MediaMetadata.Value.Title; }
            set { MediaMetadata.Value.Title = value; }
        }

        public int ForiegnId
        {
            get { return MediaMetadata.Value.ForiegnId; }
            set { MediaMetadata.Value.ForiegnId = value; }
        }

        public int Year
        {
            get { return MediaMetadata.Value.Year; }
            set { MediaMetadata.Value.Year = value; }
        }

        public string FolderName()
        {
            if (Path.IsNullOrWhiteSpace())
            {
                return "";
            }

            //Well what about Path = Null?
            //return new DirectoryInfo(Path).Name;
            return Path;
        }

        public bool IsAvailable(int delay = 0)
        {
            //the below line is what was used before delay was implemented, could still be used for cases when delay==0
            //return (Status >= MinimumAvailability || (MinimumAvailability == MovieStatusType.PreDB && Status >= MovieStatusType.Released));

            //This more complex sequence handles the delay
            DateTime minimumAvailabilityDate;

            if ((MinimumAvailability == MovieStatusType.TBA) || (MinimumAvailability == MovieStatusType.Announced))
            {
                minimumAvailabilityDate = DateTime.MinValue;
            }
            else
            {
                if (MediaMetadata.Value.DigitalRelease.HasValue)
                {
                    minimumAvailabilityDate = MediaMetadata.Value.DigitalRelease.Value;
                }
                else
                {
                    minimumAvailabilityDate = DateTime.MaxValue;
                }
            }

            if (minimumAvailabilityDate == DateTime.MinValue || minimumAvailabilityDate == DateTime.MaxValue)
            {
                return DateTime.Now >= minimumAvailabilityDate;
            }

            return DateTime.Now >= minimumAvailabilityDate.AddDays((double)delay);
        }

        public override string ToString()
        {
            return string.Format("[{1} ({2})][{3}]", MediaMetadata.Value.Title.NullSafe(), MediaMetadata.Value.Year.NullSafe(), MediaMetadata.Value.ForiegnId);
        }

        public void ApplyChanges(Media otherMovie)
        {
            Path = otherMovie.Path;
            ProfileId = otherMovie.ProfileId;

            Monitored = otherMovie.Monitored;
            MinimumAvailability = otherMovie.MinimumAvailability;

            RootFolderPath = otherMovie.RootFolderPath;
            Tags = otherMovie.Tags;
            AddOptions = otherMovie.AddOptions;
        }
    }
}
