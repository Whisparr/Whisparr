using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies.Studios;
using NzbDrone.Core.Parser;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Studios
{
    public class StudioResource : RestResource
    {
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public string SearchTitle { get; set; }
        public string ForeignId { get; set; }
        public int TmdbId { get; set; }
        public string TpdbId { get; set; }
        public string Website { get; set; }
        public string Network { get; set; }
        public List<MediaCover> Images { get; set; }
        public bool Monitored { get; set; }
        public bool MoviesMonitored { get; set; }
        public StudioStatus Status { get; set; }
        public string AfterDate { get; set; }
        public string RootFolderPath { get; set; }
        public int QualityProfileId { get; set; }
        public bool SearchOnAdd { get; set; }
        public List<string> Aliases { get; set; }
        public HashSet<int> Tags { get; set; }
        public bool HasMovies { get; set; }
        public bool HasScenes { get; set; }
        public int TotalMovieCount { get; internal set; }
        public int TotalSceneCount { get; internal set; }
        public int MovieCount { get; set; }
        public int SceneCount { get; set; }
        public List<int> Years { get; set; }
        public long SizeOnDisk { get; set; }
        public string RemotePoster { get; internal set; }
    }

    public static class StudioResourceMapper
    {
        public static StudioResource ToResource(this Studio model)
        {
            if (model == null)
            {
                return null;
            }

            return new StudioResource
            {
                Id = model.Id,
                ForeignId = model.ForeignId,
                TpdbId = model.TpdbId,
                TmdbId = model.TmdbId,
                Title = model.Title,
                SortTitle = model.SortTitle,
                SearchTitle = model.SearchTitle,
                Website = model.Website,
                Network = model.Network,
                Monitored = model.Monitored,
                MoviesMonitored = model.MoviesMonitored,
                Status = model.Status,
                AfterDate = model.AfterDate?.ToLocalTime().ToString("yyyy-MM-dd"),
                Images = model.Images,
                QualityProfileId = model.QualityProfileId,
                RootFolderPath = model.RootFolderPath,
                SearchOnAdd = model.SearchOnAdd,
                Aliases = model.Aliases,
                Tags = model.Tags
            };
        }

        public static List<StudioResource> ToResource(this IEnumerable<Studio> collections)
        {
            return collections.Select(ToResource).ToList();
        }

        public static Studio ToModel(this StudioResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Studio
            {
                Id = resource.Id,
                ForeignId = resource.ForeignId,
                Title = resource.Title,
                SortTitle = resource.SortTitle,
                SearchTitle = resource.SearchTitle,
                CleanSearchTitle = resource.SearchTitle.CleanStudioTitle(),
                Website = resource.Website,
                Network = resource.Network,
                Monitored = resource.Monitored,
                MoviesMonitored = resource.MoviesMonitored,
                Status = resource.Status,
                AfterDate = string.IsNullOrWhiteSpace(resource.AfterDate) ? null : DateTime.Parse(resource.AfterDate),
                QualityProfileId = resource.QualityProfileId,
                RootFolderPath = resource.RootFolderPath,
                SearchOnAdd = resource.SearchOnAdd,
                Aliases = resource.Aliases ?? new List<string>(),
                Tags = resource.Tags
            };
        }

        public static Studio ToModel(this StudioResource resource, Studio studio)
        {
            var updatedStudio = resource.ToModel();

            studio.ApplyChanges(updatedStudio);

            return studio;
        }
    }
}
