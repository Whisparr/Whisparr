using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies.Performers;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Performers
{
    /// <summary>Represents a performer (adult film actor/actress) in Whisparr</summary>
    public class PerformerResource : RestResource
    {
        /// <summary>The performer's full name</summary>
        public string FullName { get; set; }

        /// <summary>The performer's gender</summary>
        public Gender Gender { get; set; }

        /// <summary>The performer's hair color (optional)</summary>
        public HairColor? HairColor { get; set; }

        /// <summary>The performer's ethnicity (optional)</summary>
        public Ethnicity? Ethnicity { get; set; }

        /// <summary>The performer's career status (active, retired, etc.)</summary>
        public PerformerStatus Status { get; set; }

        /// <summary>Four-digit Year the performer started their career (optional)</summary>
        public int? CareerStart { get; set; }

        /// <summary>Year the performer ended their career (optional)</summary>
        public int? CareerEnd { get; set; }

        /// <summary>Current age of the performer (optional)</summary>
        public int? Age { get; set; }

        /// <summary>External foreign ID from metadata source (e.g., StashDB ID)</summary>
        public string ForeignId { get; set; }

        /// <summary>The Porn Database (TPDB) identifier</summary>
        public string TpdbId { get; set; }

        /// <summary>Collection of performer images (posters, headshots, etc.)</summary>
        public List<MediaCover> Images { get; set; }

        /// <summary>Whether this performer is being monitored for new content</summary>
        public bool Monitored { get; set; }

        /// <summary>Whether movies featuring this performer are monitored</summary>
        public bool MoviesMonitored { get; set; }

        /// <summary>Root folder path where performer content is stored</summary>
        public string RootFolderPath { get; set; }

        /// <summary>ID of the quality profile to use for this performer's content</summary>
        public int QualityProfileId { get; set; }

        /// <summary>Whether to automatically search for content when adding this performer</summary>
        public bool SearchOnAdd { get; set; }

        /// <summary>Set of tag IDs associated with this performer</summary>
        public HashSet<int> Tags { get; set; }

        /// <summary>URL of the remote poster image from StashDb/TMDb/TPDb</summary>
        public string RemotePoster { get; internal set; }

        /// <summary>Date and time when this performer was added to Whisparr</summary>
        public DateTime Added { get; internal set; }

        /// <summary>Indicates if the performer has any associated movies</summary>
        public bool HasMovies { get; set; }

        /// <summary>Indicates if the performer has any associated scenes</summary>
        public bool HasScenes { get; set; }

        /// <summary>Total number of movies associated with this performer</summary>
        public int TotalMovieCount
        {
            get;
            internal set;
        }

        /// <summary>Total number of scenes associated with this performer</summary>
        public int TotalSceneCount
        {
            get;
            internal set;
        }

        /// <summary>Number of scenes from this performer</summary>
        public int SceneCount { get; set; }

        /// <summary>List of minimal-property studio objects associated with this performer</summary>
        public List<PerformerStudioResource> Studios { get; set; }
        public long SizeOnDisk { get; set; }
    }

    public class PerformerStudioResource
    {
        /// <summary>The studio's name</summary>
        public string Title { get; set; }

        /// <summary>Foreign ID from Stash metadata source</summary>
        public string ForeignId { get; set; }
    }

    /// <summary>Provides mapping functions between Performer model and PerformerResource</summary>
    public static class PerformerResourceMapper
    {
        /// <summary>Converts a Performer model to a PerformerResource</summary>
        /// <param name="model">The Performer model to map from</param>
        /// <returns>A PerformerResource object</returns>
        public static PerformerResource ToResource(this Performer model)
        {
            if (model == null)
            {
                return null;
            }

            return new PerformerResource
            {
                Id = model.Id,
                ForeignId = model.ForeignId,
                TpdbId = model.TpdbId,
                Gender = model.Gender,
                Age = model.Age,
                Ethnicity = model.Ethnicity,
                HairColor = model.HairColor,
                Status = model.Status,
                CareerStart = model.CareerStart,
                CareerEnd = model.CareerEnd,
                FullName = model.Name,
                Monitored = model.Monitored,
                MoviesMonitored = model.MoviesMonitored,
                Images = model.Images,
                QualityProfileId = model.QualityProfileId,
                RootFolderPath = model.RootFolderPath,
                SearchOnAdd = model.SearchOnAdd,
                Tags = model.Tags
            };
        }

        /// <summary>Maps a collection of Performer models to a list of PerformerResources</summary>
        /// <param name="collections"></param>
        /// <returns>A list of PerformerResource objects</returns>
        public static List<PerformerResource> ToResource(this IEnumerable<Performer> collections)
        {
            return collections.Select(ToResource).ToList();
        }

        /// <summary>Maps a PerformerResource to a Performer model</summary>
        /// <param name="resource">The PerformerResource to map</param>
        /// <returns>The mapped Performer model</returns>
        public static Performer ToModel(this PerformerResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Performer
            {
                Id = resource.Id,
                ForeignId = resource.ForeignId,
                Name = resource.FullName,
                Monitored = resource.Monitored,
                MoviesMonitored = resource.MoviesMonitored,
                QualityProfileId = resource.QualityProfileId,
                RootFolderPath = resource.RootFolderPath,
                SearchOnAdd = resource.SearchOnAdd,
                Tags = resource.Tags,
                Added = resource.Added,
            };
        }

        /// <summary>Updates an existing Performer model with data from a PerformerResource</summary>
        /// <param name="resource">The PerformerResource containing updated data</param>
        /// <param name="performer">The existing Performer model to update</param>
        /// <returns>The updated Performer model</returns>
        public static Performer ToModel(this PerformerResource resource, Performer performer)
        {
            var updatedPerformer = resource.ToModel();

            performer.ApplyChanges(updatedPerformer);

            return performer;
        }
    }
}
